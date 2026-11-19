import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import {
  getRevenueReport,
  getTopProductsReport,
  getShiftsReport,
  downloadReportCsv,
} from '../../api/reports';
import type {
  RevenueSummary,
  TopProductsReport,
  ShiftsReport,
  ReportFilterParams,
} from '../../api/reports';
import './ReportDashboardScreen.css';

// SVG Icons
const IconTrendingUp = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="23 6 13.5 15.5 8.5 10.5 1 18" />
    <polyline points="17 6 23 6 23 12" />
  </svg>
);

const IconShoppingBag = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z" />
    <line x1="3" y1="6" x2="21" y2="6" />
    <path d="M16 10a4 4 0 0 1-8 0" />
  </svg>
);

const IconPackage = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="16.5" y1="9.4" x2="7.5" y2="4.21" />
    <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
    <polyline points="3.27 6.96 12 12.01 20.73 6.96" />
    <line x1="12" y1="22.08" x2="12" y2="12" />
  </svg>
);

const IconDollar = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="12" y1="1" x2="12" y2="23" />
    <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
  </svg>
);

const IconDownload = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
    <polyline points="7 10 12 15 17 10" />
    <line x1="12" y1="15" x2="12" y2="3" />
  </svg>
);

const IconRefresh = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M23 4v6h-6" />
    <path d="M1 20v-6h6" />
    <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15" />
  </svg>
);

const formatCurrency = (amount: number) =>
  `${Math.round(amount).toLocaleString('vi-VN')} đ`;

const formatShortCurrency = (val: number) => {
  if (val >= 1000000) return `${(val / 1000000).toFixed(1)}Tr`;
  if (val >= 1000) return `${(val / 1000).toFixed(0)}K`;
  return `${val}`;
};

const formatDateTime = (isoDate: string) => {
  const d = new Date(isoDate);
  if (Number.isNaN(d.getTime())) return isoDate;
  return d.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

export const ReportDashboardScreen: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  // Role validation: Quản lý only (UC14 / UC15)
  const isManager = useMemo(() => {
    const role = user?.vaiTro?.toLowerCase() || '';
    return role.includes('quản lý') || role.includes('admin') || role.includes('quanly');
  }, [user]);

  // States
  const [activePeriod, setActivePeriod] = useState<ReportFilterParams['period']>('thisMonth');
  const [customFrom, setCustomFrom] = useState<string>('');
  const [customTo, setCustomTo] = useState<string>('');
  const [activeTab, setActiveTab] = useState<'revenue' | 'products' | 'shifts'>('revenue');
  const [chartType, setChartType] = useState<'bar' | 'line'>('bar');
  const [hoveredPoint, setHoveredPoint] = useState<number | null>(null);

  // Data states
  const [revenueData, setRevenueData] = useState<RevenueSummary | null>(null);
  const [topProductsData, setTopProductsData] = useState<TopProductsReport | null>(null);
  const [shiftsData, setShiftsData] = useState<ShiftsReport | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isExporting, setIsExporting] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  const filterParams = useMemo<ReportFilterParams>(() => {
    if (activePeriod === 'custom' && customFrom && customTo) {
      return { period: 'custom', fromDate: customFrom, toDate: customTo };
    }
    return { period: activePeriod };
  }, [activePeriod, customFrom, customTo]);

  // Load reports in effect without synchronous setState
  useEffect(() => {
    if (!isManager) return;
    let ignore = false;

    Promise.all([
      getRevenueReport(filterParams),
      getTopProductsReport(5, filterParams),
      getShiftsReport(filterParams),
    ])
      .then(([rev, top, shifts]) => {
        if (!ignore) {
          setRevenueData(rev);
          setTopProductsData(top);
          setShiftsData(shifts);
          setIsLoading(false);
        }
      })
      .catch((err: unknown) => {
        if (!ignore) {
          const msg =
            (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
            'Không thể tải dữ liệu báo cáo thống kê từ máy chủ.';
          setError(msg);
          setIsLoading(false);
        }
      });

    return () => {
      ignore = true;
    };
  }, [isManager, filterParams]);

  // Manual refresh button handler
  const handleRefresh = useCallback(async () => {
    setIsLoading(true);
    setError('');
    try {
      const [rev, top, shifts] = await Promise.all([
        getRevenueReport(filterParams),
        getTopProductsReport(5, filterParams),
        getShiftsReport(filterParams),
      ]);
      setRevenueData(rev);
      setTopProductsData(top);
      setShiftsData(shifts);
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Không thể tải dữ liệu báo cáo thống kê từ máy chủ.';
      setError(msg);
    } finally {
      setIsLoading(false);
    }
  }, [filterParams]);

  // Handle Export CSV
  const handleExport = async (type: 'revenue' | 'top-products' | 'shifts') => {
    setIsExporting(true);
    try {
      await downloadReportCsv(type, filterParams);
    } catch {
      alert('Có lỗi xảy ra khi xuất file báo cáo.');
    } finally {
      setIsExporting(false);
    }
  };

  // If unauthorized employee tries to access (UC14 security check)
  if (!isManager) {
    return (
      <div className="reports-screen">
        <header className="reports-header">
          <div className="brand-badge">
            <span className="brand-title">MERA · SOUVENIR</span>
            <span className="brand-subtitle">Hệ thống quản lý bán hàng</span>
          </div>
          <button type="button" className="nav-pill-btn" onClick={() => navigate('/pos')}>
            ← Quay lại Bán hàng (POS)
          </button>
        </header>
        <div className="empty-state-box" style={{ marginTop: '80px' }}>
          <div className="empty-state-icon">🔒</div>
          <h2>Truy cập bị hạn chế (Quyền Quản lý)</h2>
          <p>Tài khoản nhân viên không có quyền xem báo cáo doanh thu &amp; số liệu tài chính.</p>
          <button
            type="button"
            className="btn-export"
            style={{ marginTop: '16px' }}
            onClick={() => navigate('/pos')}
          >
            Quay lại quầy bán hàng POS
          </button>
        </div>
      </div>
    );
  }

  // Calculate SVG Chart Dimensions & Coordinates
  const timeline = revenueData?.timeline || [];
  const maxRevenue = Math.max(1, ...timeline.map((p) => p.doanhThu));
  // Round up max for nice axis ticks
  const chartHeight = 220;
  const chartWidth = 760;
  const paddingLeft = 60;
  const paddingRight = 20;
  const paddingTop = 20;
  const paddingBottom = 40;
  const innerWidth = chartWidth - paddingLeft - paddingRight;
  const innerHeight = chartHeight - paddingTop - paddingBottom;

  const pointsCount = timeline.length;
  const stepX = pointsCount > 1 ? innerWidth / (pointsCount - 1) : innerWidth;
  const barWidth = Math.max(8, Math.min(36, (innerWidth / (pointsCount || 1)) * 0.55));

  // Top products donut slices colors
  const donutColors = ['#C8A27A', '#3498DB', '#2ECC71', '#E67E22', '#9B59B6'];

  return (
    <div className="reports-screen">
      {/* Header bar */}
      <header className="reports-header">
        <div className="reports-header-left">
          <div className="brand-badge">
            <span className="brand-title">MERA · SOUVENIR</span>
            <span className="brand-subtitle">Báo cáo &amp; Thống kê · UC14</span>
          </div>

          <nav className="header-nav">
            <button type="button" className="nav-pill-btn" onClick={() => navigate('/pos')}>
              <span>💳</span>
              <span>Bán hàng POS</span>
            </button>
            <button type="button" className="nav-pill-btn" onClick={() => navigate('/customers')}>
              <span>👥</span>
              <span>Khách hàng</span>
            </button>
            <button type="button" className="nav-pill-btn" onClick={() => navigate('/returns')}>
              <span>📦</span>
              <span>Trả hàng</span>
            </button>
            <button type="button" className="nav-pill-btn" onClick={() => navigate('/users')}>
              <span>🛡️</span>
              <span>Nhân sự</span>
            </button>
          </nav>
        </div>

        <div className="reports-header-right">
          <div className="user-profile-badge">
            <div className="user-avatar-circle">
              {(user?.hoTen || user?.tenDangNhap || 'QL').charAt(0).toUpperCase()}
            </div>
            <div className="user-info-text">
              <span className="user-display-name">
                {user?.hoTen || user?.tenDangNhap || 'Quản lý'}
              </span>
              <span className="user-role-tag">Quản lý</span>
            </div>
          </div>
          <button
            type="button"
            className="btn-header-logout"
            onClick={() => {
              logout();
              navigate('/login');
            }}
          >
            Đăng xuất
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="reports-container">
        {/* Top bar with filters & export */}
        <div className="reports-top-bar">
          <div className="page-title-box">
            <h1>Báo cáo Doanh thu &amp; Thống kê Bán hàng</h1>
            <p>
              Đối soát tổng hợp số liệu hóa đơn bán lẻ, hiệu suất sản phẩm và các ca làm việc.
            </p>
          </div>

          <div className="filter-actions-bar">
            {/* Period selector */}
            <div className="period-tabs">
              <button
                type="button"
                className={`period-tab-btn ${activePeriod === 'today' ? 'active' : ''}`}
                onClick={() => setActivePeriod('today')}
              >
                Hôm nay
              </button>
              <button
                type="button"
                className={`period-tab-btn ${activePeriod === '7days' ? 'active' : ''}`}
                onClick={() => setActivePeriod('7days')}
              >
                7 ngày qua
              </button>
              <button
                type="button"
                className={`period-tab-btn ${activePeriod === '30days' ? 'active' : ''}`}
                onClick={() => setActivePeriod('30days')}
              >
                30 ngày qua
              </button>
              <button
                type="button"
                className={`period-tab-btn ${activePeriod === 'thisMonth' ? 'active' : ''}`}
                onClick={() => setActivePeriod('thisMonth')}
              >
                Tháng này
              </button>
              <button
                type="button"
                className={`period-tab-btn ${activePeriod === 'lastMonth' ? 'active' : ''}`}
                onClick={() => setActivePeriod('lastMonth')}
              >
                Tháng trước
              </button>
              <button
                type="button"
                className={`period-tab-btn ${activePeriod === 'custom' ? 'active' : ''}`}
                onClick={() => setActivePeriod('custom')}
              >
                Tùy chọn
              </button>
            </div>

            {/* Custom date range if active */}
            {activePeriod === 'custom' && (
              <div className="custom-date-box">
                <span>Từ:</span>
                <input
                  type="date"
                  value={customFrom}
                  onChange={(e) => setCustomFrom(e.target.value)}
                />
                <span>Đến:</span>
                <input
                  type="date"
                  value={customTo}
                  onChange={(e) => setCustomTo(e.target.value)}
                />
              </div>
            )}

            <button
              type="button"
              className="btn-refresh"
              onClick={handleRefresh}
              title="Tải lại dữ liệu mới nhất"
            >
              <IconRefresh />
            </button>

            <div className="btn-export-group">
              <button
                type="button"
                className="btn-export"
                disabled={isExporting}
                onClick={() =>
                  handleExport(
                    activeTab === 'products'
                      ? 'top-products'
                      : activeTab === 'shifts'
                      ? 'shifts'
                      : 'revenue'
                  )
                }
              >
                <IconDownload />
                <span>{isExporting ? 'Đang xuất...' : 'Xuất File Excel'}</span>
              </button>
            </div>
          </div>
        </div>

        {/* Error message */}
        {error && (
          <div
            style={{
              padding: '14px 20px',
              backgroundColor: '#FEEBEA',
              color: '#C25E5E',
              borderRadius: '10px',
              marginBottom: '20px',
              fontSize: '14px',
            }}
          >
            {error}
          </div>
        )}

        {/* 3 Thẻ Số Liệu Tổng Quan (Key Metrics Cards) */}
        <section className="overview-metrics-grid">
          {/* Card 1: Tổng Doanh Thu */}
          <div className="metric-card primary">
            <div className="metric-top">
              <span className="metric-label">Tổng doanh thu</span>
              <div className="metric-icon-bubble">
                <IconDollar />
              </div>
            </div>
            <div className="metric-value">
              {isLoading ? '...' : formatCurrency(revenueData?.tongDoanhThu || 0)}
            </div>
            <div className="metric-subtitle">
              <span>Doanh thu thuần sau chiết khấu &amp; đổi trả</span>
            </div>
          </div>

          {/* Card 2: Tổng Đơn Hàng */}
          <div className="metric-card orders">
            <div className="metric-top">
              <span className="metric-label">Tổng đơn hàng</span>
              <div className="metric-icon-bubble">
                <IconShoppingBag />
              </div>
            </div>
            <div className="metric-value">
              {isLoading ? '...' : `${revenueData?.tongDonHang || 0} đơn`}
            </div>
            <div className="metric-subtitle">
              <span className="metric-pill">
                TB: {formatCurrency(revenueData?.doanhThuTrungBinhMoiDon || 0)} / đơn
              </span>
            </div>
          </div>

          {/* Card 3: Tổng Tồn Kho */}
          <div className="metric-card inventory">
            <div className="metric-top">
              <span className="metric-label">Tổng tồn kho</span>
              <div className="metric-icon-bubble">
                <IconPackage />
              </div>
            </div>
            <div className="metric-value">
              {isLoading ? '...' : `${revenueData?.tongTonKho || 0} món`}
            </div>
            <div className="metric-subtitle">
              <span>Giá trị tồn: {formatCurrency(revenueData?.tongGiaTriTonKho || 0)}</span>
            </div>
          </div>

          {/* Card 4: Lợi Nhuận Ước Tính */}
          <div className="metric-card profit">
            <div className="metric-top">
              <span className="metric-label">Lợi nhuận gộp</span>
              <div className="metric-icon-bubble">
                <IconTrendingUp />
              </div>
            </div>
            <div className="metric-value">
              {isLoading ? '...' : formatCurrency(revenueData?.tongLoiNhuanUocTinh || 0)}
            </div>
            <div className="metric-subtitle">
              <span>Đã trừ giá vốn sản phẩm ({revenueData?.tongSanPhamDaBan || 0} món đã bán)</span>
            </div>
          </div>
        </section>

        {/* Section Navigation Tabs */}
        <div className="reports-section-nav">
          <button
            type="button"
            className={`section-nav-tab ${activeTab === 'revenue' ? 'active' : ''}`}
            onClick={() => setActiveTab('revenue')}
          >
            📊 Biểu đồ Doanh thu &amp; Xu hướng
          </button>
          <button
            type="button"
            className={`section-nav-tab ${activeTab === 'products' ? 'active' : ''}`}
            onClick={() => setActiveTab('products')}
          >
            🏆 Top 5 Sản phẩm bán chạy nhất
          </button>
          <button
            type="button"
            className={`section-nav-tab ${activeTab === 'shifts' ? 'active' : ''}`}
            onClick={() => setActiveTab('shifts')}
          >
            🕒 Đối soát &amp; Doanh thu theo ca làm việc
          </button>
        </div>

        {/* Tab 1: Biểu đồ doanh thu */}
        {activeTab === 'revenue' && (
          <div className="chart-panel-card">
            <div className="chart-panel-header">
              <div className="chart-title-box">
                <h3>Biểu đồ Diễn biến Doanh thu ({revenueData?.kyBaoCao || 'Kỳ này'})</h3>
                <p>Số liệu dựa trên các hóa đơn đã thanh toán thành công tại quầy POS.</p>
              </div>

              <div className="chart-type-toggles">
                <button
                  type="button"
                  className={`chart-type-btn ${chartType === 'bar' ? 'active' : ''}`}
                  onClick={() => setChartType('bar')}
                >
                  Biểu đồ cột
                </button>
                <button
                  type="button"
                  className={`chart-type-btn ${chartType === 'line' ? 'active' : ''}`}
                  onClick={() => setChartType('line')}
                >
                  Đường xu hướng
                </button>
              </div>
            </div>

            {isLoading ? (
              <div className="loading-spinner-box">
                <div className="spinner" />
                <span>Đang tổng hợp dữ liệu doanh thu...</span>
              </div>
            ) : timeline.length === 0 ? (
              <div className="empty-state-box">
                <div className="empty-state-icon">📉</div>
                <h3>Chưa có giao dịch phát sinh trong khoảng thời gian này</h3>
                <p>Doanh thu ghi nhận = 0 đ. Hãy thực hiện bán hàng tại POS để số liệu cập nhật.</p>
              </div>
            ) : (
              <div className="svg-chart-container">
                <svg viewBox={`0 0 ${chartWidth} ${chartHeight}`} preserveAspectRatio="none">
                  <defs>
                    <linearGradient id="barGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#C8A27A" stopOpacity="0.95" />
                      <stop offset="100%" stopColor="#D9BF9F" stopOpacity="0.4" />
                    </linearGradient>
                    <linearGradient id="areaGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#C8A27A" stopOpacity="0.35" />
                      <stop offset="100%" stopColor="#C8A27A" stopOpacity="0.0" />
                    </linearGradient>
                  </defs>

                  {/* Horizontal Grid lines */}
                  {[0, 0.25, 0.5, 0.75, 1].map((pct, idx) => {
                    const y = paddingTop + innerHeight * (1 - pct);
                    const val = maxRevenue * pct;
                    return (
                      <g key={idx}>
                        <line
                          x1={paddingLeft}
                          y1={y}
                          x2={chartWidth - paddingRight}
                          y2={y}
                          stroke="#EAE3DB"
                          strokeDasharray="4 4"
                          strokeWidth="1"
                        />
                        <text
                          x={paddingLeft - 8}
                          y={y + 4}
                          textAnchor="end"
                          fontSize="10"
                          fill="#8C7E74"
                        >
                          {formatShortCurrency(val)}
                        </text>
                      </g>
                    );
                  })}

                  {/* Bar Chart rendering */}
                  {chartType === 'bar' &&
                    timeline.map((p, idx) => {
                      const x =
                        pointsCount === 1
                          ? paddingLeft + innerWidth / 2 - barWidth / 2
                          : paddingLeft + idx * stepX - barWidth / 2;
                      const barH = (p.doanhThu / maxRevenue) * innerHeight;
                      const y = paddingTop + innerHeight - barH;
                      const isHovered = hoveredPoint === idx;

                      return (
                        <g
                          key={p.date}
                          onMouseEnter={() => setHoveredPoint(idx)}
                          onMouseLeave={() => setHoveredPoint(null)}
                          style={{ cursor: 'pointer' }}
                        >
                          <rect
                            x={x}
                            y={y}
                            width={barWidth}
                            height={Math.max(2, barH)}
                            rx="4"
                            ry="4"
                            fill={isHovered ? '#B3885C' : 'url(#barGradient)'}
                            stroke={isHovered ? '#8C6239' : 'none'}
                            strokeWidth="1.5"
                          />
                          {/* Label on X Axis */}
                          <text
                            x={x + barWidth / 2}
                            y={chartHeight - 12}
                            textAnchor="middle"
                            fontSize="10"
                            fontWeight={isHovered ? '700' : '500'}
                            fill={isHovered ? '#2B2523' : '#8C7E74'}
                          >
                            {p.label}
                          </text>
                        </g>
                      );
                    })}

                  {/* Line / Area Chart rendering */}
                  {chartType === 'line' && (
                    <>
                      {/* Area Fill */}
                      <path
                        d={`M ${paddingLeft} ${paddingTop + innerHeight} ${timeline
                          .map((p, idx) => {
                            const x = paddingLeft + idx * stepX;
                            const y = paddingTop + innerHeight - (p.doanhThu / maxRevenue) * innerHeight;
                            return `L ${x} ${y}`;
                          })
                          .join(' ')} L ${paddingLeft + (pointsCount - 1) * stepX} ${
                          paddingTop + innerHeight
                        } Z`}
                        fill="url(#areaGradient)"
                      />

                      {/* Main Stroke */}
                      <path
                        d={timeline
                          .map((p, idx) => {
                            const x = paddingLeft + idx * stepX;
                            const y = paddingTop + innerHeight - (p.doanhThu / maxRevenue) * innerHeight;
                            return `${idx === 0 ? 'M' : 'L'} ${x} ${y}`;
                          })
                          .join(' ')}
                        fill="none"
                        stroke="#C8A27A"
                        strokeWidth="3"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />

                      {/* Data Dots */}
                      {timeline.map((p, idx) => {
                        const x = paddingLeft + idx * stepX;
                        const y = paddingTop + innerHeight - (p.doanhThu / maxRevenue) * innerHeight;
                        const isHovered = hoveredPoint === idx;

                        return (
                          <g
                            key={p.date}
                            onMouseEnter={() => setHoveredPoint(idx)}
                            onMouseLeave={() => setHoveredPoint(null)}
                            style={{ cursor: 'pointer' }}
                          >
                            <circle
                              cx={x}
                              cy={y}
                              r={isHovered ? 6 : 4}
                              fill="#FFFFFF"
                              stroke="#C8A27A"
                              strokeWidth={isHovered ? 3 : 2}
                            />
                            <text
                              x={x}
                              y={chartHeight - 12}
                              textAnchor="middle"
                              fontSize="10"
                              fontWeight={isHovered ? '700' : '500'}
                              fill={isHovered ? '#2B2523' : '#8C7E74'}
                            >
                              {p.label}
                            </text>
                          </g>
                        );
                      })}
                    </>
                  )}

                  {/* Tooltip Overlay */}
                  {hoveredPoint !== null && timeline[hoveredPoint] && (
                    <g className="chart-tooltip-hover">
                      {(() => {
                        const p = timeline[hoveredPoint];
                        const x =
                          pointsCount === 1
                            ? paddingLeft + innerWidth / 2
                            : paddingLeft + hoveredPoint * stepX;
                        const tooltipX = Math.min(chartWidth - 110, Math.max(70, x));
                        const y =
                          paddingTop + innerHeight - (p.doanhThu / maxRevenue) * innerHeight - 35;

                        return (
                          <g>
                            <rect
                              x={tooltipX - 60}
                              y={Math.max(10, y)}
                              width="120"
                              height="44"
                              rx="6"
                              ry="6"
                              fill="#2B2523"
                              opacity="0.9"
                            />
                            <text
                              x={tooltipX}
                              y={Math.max(10, y) + 16}
                              textAnchor="middle"
                              fill="#FFFFFF"
                              fontSize="10"
                              fontWeight="600"
                            >
                              {p.label} ({p.soDonHang} đơn)
                            </text>
                            <text
                              x={tooltipX}
                              y={Math.max(10, y) + 32}
                              textAnchor="middle"
                              fill="#E8C9A6"
                              fontSize="11"
                              fontWeight="700"
                            >
                              {formatCurrency(p.doanhThu)}
                            </text>
                          </g>
                        );
                      })()}
                    </g>
                  )}
                </svg>
              </div>
            )}
          </div>
        )}

        {/* Tab 2: Top 5 Sản Phẩm Bán Chạy */}
        {activeTab === 'products' && (
          <div className="dual-dashboard-grid">
            <div className="top-products-card">
              <div className="top-products-header">
                <h3>Xếp hạng Top 5 Sản phẩm bán chạy nhất</h3>
                <span className="metric-pill">
                  {topProductsData?.items.reduce((s, i) => s + i.soLuongDaBan, 0) || 0} món bán ra
                </span>
              </div>

              {isLoading ? (
                <div className="loading-spinner-box">
                  <div className="spinner" />
                  <span>Đang tải xếp hạng sản phẩm...</span>
                </div>
              ) : topProductsData?.items.length === 0 ? (
                <div className="empty-state-box">
                  <p>Chưa có dữ liệu sản phẩm bán ra.</p>
                </div>
              ) : (
                <div className="top-products-list">
                  {topProductsData?.items.map((item, idx) => (
                    <div key={item.sanPhamId} className="top-product-row">
                      <div className={`product-rank-badge rank-${idx + 1}`}>
                        {idx + 1}
                      </div>

                      <img
                        src={item.hinhAnh || '/products/teddy.png'}
                        alt={item.tenSP}
                        className="product-mini-thumb"
                        onError={(e) => {
                          (e.target as HTMLImageElement).src =
                            'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="44" height="44" viewBox="0 0 24 24" fill="none" stroke="%23C8A27A" stroke-width="1.5"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>';
                        }}
                      />

                      <div className="product-row-info">
                        <div className="product-row-name" title={item.tenSP}>
                          {item.tenSP}
                        </div>
                        <div className="product-row-category">
                          Mã: {item.maSP} · {item.danhMuc}
                        </div>
                      </div>

                      <div className="product-row-stat">
                        <div className="product-stat-revenue">
                          {formatCurrency(item.tongDoanhThu)}
                        </div>
                        <div className="product-stat-qty">
                          Đã bán: <strong>{item.soLuongDaBan}</strong> món ({item.phanTramDoanhThu}%)
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Donut Chart / Share proportion visualizer */}
            <div className="top-products-card">
              <div className="top-products-header">
                <h3>Tỷ trọng Doanh thu Top Sản phẩm</h3>
                <span className="metric-pill">Cơ cấu danh mục</span>
              </div>

              <div className="donut-chart-container">
                <svg width="180" height="180" viewBox="0 0 100 100">
                  {(() => {
                    const items = topProductsData?.items || [];
                    const totalRev = items.reduce((s, i) => s + i.tongDoanhThu, 0) || 1;
                    let accumulatedAngle = 0;

                    return items.map((item, idx) => {
                      const sliceAngle = (item.tongDoanhThu / totalRev) * 360;
                      if (sliceAngle <= 0) return null;

                      const startAngle = accumulatedAngle;
                      const endAngle = accumulatedAngle + sliceAngle;
                      accumulatedAngle = endAngle;

                      // Convert polar to cartesian
                      const r = 38;
                      const cx = 50;
                      const cy = 50;
                      const x1 = cx + r * Math.cos((Math.PI * (startAngle - 90)) / 180);
                      const y1 = cy + r * Math.sin((Math.PI * (startAngle - 90)) / 180);
                      const x2 = cx + r * Math.cos((Math.PI * (endAngle - 90)) / 180);
                      const y2 = cy + r * Math.sin((Math.PI * (endAngle - 90)) / 180);
                      const largeArc = sliceAngle > 180 ? 1 : 0;

                      return (
                        <path
                          key={item.sanPhamId}
                          d={`M ${cx} ${cy} L ${x1} ${y1} A ${r} ${r} 0 ${largeArc} 1 ${x2} ${y2} Z`}
                          fill={donutColors[idx % donutColors.length]}
                          stroke="#FFFFFF"
                          strokeWidth="1.5"
                        />
                      );
                    });
                  })()}
                  {/* Center cutout circle for Donut effect */}
                  <circle cx="50" cy="50" r="22" fill="#FFFFFF" />
                  <text x="50" y="52" textAnchor="middle" fontSize="8" fontWeight="700" fill="#2B2523">
                    TOP 5
                  </text>
                </svg>

                <div className="donut-legend">
                  {topProductsData?.items.map((item, idx) => (
                    <div key={item.sanPhamId} className="donut-legend-item">
                      <div
                        className="donut-color-dot"
                        style={{ backgroundColor: donutColors[idx % donutColors.length] }}
                      />
                      <span>
                        {item.tenSP.slice(0, 16)}... ({item.phanTramDoanhThu}%)
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Tab 3: Đối soát Ca làm việc */}
        {activeTab === 'shifts' && (
          <div className="shifts-table-card">
            <div className="shifts-table-header">
              <h3>Bảng Đối soát &amp; Doanh thu từng Ca làm việc</h3>
              <span className="metric-pill">
                Tổng: {formatCurrency(shiftsData?.tongDoanhThuCacCa || 0)}
              </span>
            </div>

            {isLoading ? (
              <div className="loading-spinner-box">
                <div className="spinner" />
                <span>Đang tải danh sách ca...</span>
              </div>
            ) : shiftsData?.items.length === 0 ? (
              <div className="empty-state-box">
                <p>Chưa có ca làm việc nào trong khoảng thời gian này.</p>
              </div>
            ) : (
              <div className="table-responsive">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Mã Ca</th>
                      <th>Nhân viên</th>
                      <th>Thời gian mở ca</th>
                      <th>Thời gian chốt</th>
                      <th>Bàn giao</th>
                      <th>Doanh thu TM</th>
                      <th>Chuyển khoản</th>
                      <th>Tổng ca</th>
                      <th>Kiểm đếm</th>
                      <th>Chênh lệch</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {shiftsData?.items.map((s) => {
                      const isClosed = s.trangThai === 'DA_CHOT';
                      const isDiscrepancy = s.trangThai === 'CHO_XU_LY_CHENH_LECH';

                      return (
                        <tr key={s.caLamViecId}>
                          <td>
                            <strong>#{s.caLamViecId}</strong>
                          </td>
                          <td>{s.tenNhanVien}</td>
                          <td>{formatDateTime(s.thoiGianMoCa)}</td>
                          <td>
                            {s.thoiGianChotCa ? formatDateTime(s.thoiGianChotCa) : (
                              <span style={{ color: '#1565C0', fontWeight: 600 }}>Đang mở</span>
                            )}
                          </td>
                          <td>{formatCurrency(s.tienMatBanGiao)}</td>
                          <td>{formatCurrency(s.tongDoanhThuTienMat)}</td>
                          <td>{formatCurrency(s.tongDoanhThuChuyenKhoan)}</td>
                          <td>
                            <strong>{formatCurrency(s.tongDoanhThuCa)}</strong>
                          </td>
                          <td>
                            {s.tienMatThucTe !== null && s.tienMatThucTe !== undefined
                              ? formatCurrency(s.tienMatThucTe)
                              : '—'}
                          </td>
                          <td>
                            {s.chenhLech === 0 ? (
                              <span className="diff-zero">0 đ (Khớp)</span>
                            ) : s.chenhLech > 0 ? (
                              <span className="diff-positive">+{formatCurrency(s.chenhLech)} (Thừa)</span>
                            ) : (
                              <span className="diff-negative">{formatCurrency(s.chenhLech)} (Thiếu)</span>
                            )}
                          </td>
                          <td>
                            <span
                              className={`shift-status-badge ${
                                isClosed
                                  ? 'closed'
                                  : isDiscrepancy
                                  ? 'discrepancy'
                                  : 'open'
                              }`}
                            >
                              {isClosed
                                ? 'Đã chốt'
                                : isDiscrepancy
                                ? 'Chờ giải trình'
                                : 'Đang làm'}
                            </span>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  );
};
