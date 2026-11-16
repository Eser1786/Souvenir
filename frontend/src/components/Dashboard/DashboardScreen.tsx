import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import './DashboardScreen.css';

export const DashboardScreen: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const roleClass = user?.vaiTro?.toLowerCase().includes('quản lý') || user?.vaiTro?.toLowerCase().includes('admin')
    ? 'quanly'
    : 'nhanvien';

  return (
    <div className="dashboard-container">
      {/* Header bar */}
      <header className="dashboard-header">
        <div className="dashboard-brand">
          <span className="brand-logo-text">MERA · SOUVENIR</span>
          <span className="store-badge">Cửa hàng 01</span>
        </div>

        <div className="user-profile-bar">
          <div className="user-info-text">
            <span className="user-name">{user?.hoTen || user?.tenDangNhap || 'Người dùng'}</span>
            <span className={`role-badge ${roleClass}`}>
              {user?.vaiTro || 'Nhân viên'}
            </span>
          </div>
          <button type="button" className="logout-btn" onClick={handleLogout}>
            Đăng xuất
          </button>
        </div>
      </header>

      {/* Main dashboard content */}
      <main className="dashboard-content">
        <div className="welcome-banner">
          <div>
            <h1 className="welcome-title">Xin chào, {user?.hoTen || user?.tenDangNhap}!</h1>
            <p className="welcome-subtitle">
              Ca làm việc hiện tại tại Cửa hàng 01 · Hệ thống quản lý bán hàng &amp; kho nội bộ.
            </p>
          </div>
        </div>

        <div className="quick-modules-grid">
          <div className="module-card">
            <div className="module-icon-box">💳</div>
            <h2 className="module-title">Bán hàng (POS)</h2>
            <p className="module-desc">
              Tạo đơn bán nhanh, áp dụng khuyến mãi, thanh toán và in hóa đơn cho khách hàng.
            </p>
            <span className="module-btn">Vào giao diện bán hàng →</span>
          </div>

          <div className="module-card">
            <div className="module-icon-box">📦</div>
            <h2 className="module-title">Quản lý kho</h2>
            <p className="module-desc">
              Theo dõi số lượng tồn kho quà tặng, tạo phiếu nhập/xuất và kiểm kê định kỳ.
            </p>
            <span className="module-btn">Xem danh mục kho →</span>
          </div>

          <div className="module-card">
            <div className="module-icon-box">👥</div>
            <h2 className="module-title">Khách hàng &amp; Thành viên</h2>
            <p className="module-desc">
              Tra cứu thông tin khách hàng thân thiết, tích điểm đổi quà và chăm sóc khách.
            </p>
            <span className="module-btn">Xem danh sách khách hàng →</span>
          </div>
        </div>
      </main>
    </div>
  );
};
