import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import axios from 'axios';
import './LoginScreen.css';

export const LoginScreen: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [tenDangNhap, setTenDangNhap] = useState('');
  const [matKhau, setMatKhau] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isLocked, setIsLocked] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccessMessage(null);

    const trimmedUsername = tenDangNhap.trim();
    if (!trimmedUsername || !matKhau) {
      setError('Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.');
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await login({
        tenDangNhap: trimmedUsername,
        matKhau,
      });

      const displayName = response.hoTen || response.user?.hoTen || trimmedUsername;
      setSuccessMessage(`Đăng nhập thành công! Chào mừng ${displayName}.`);
      
      setTimeout(() => {
        navigate('/pos');
      }, 500);
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        const status = err.response?.status;
        const data = err.response?.data;

        // Check for locked account (403 or isLocked flag or specific keywords)
        const isAccountLocked =
          status === 403 ||
          data?.isLocked === true ||
          (typeof data?.message === 'string' &&
            (data.message.toLowerCase().includes('khóa') ||
              data.message.toLowerCase().includes('lock')));

        if (isAccountLocked) {
          setIsLocked(true);
          setError(null);
        } else if (status === 401) {
          setIsLocked(false);
          const remainingAttempts = data?.remainingAttempts;
          if (remainingAttempts !== undefined && remainingAttempts !== null) {
            setError(`Mật khẩu không chính xác. Bạn còn ${remainingAttempts} lần thử.`);
          } else {
            setError(data?.message || 'Tên đăng nhập hoặc mật khẩu không chính xác.');
          }
        } else if (!err.response) {
          setIsLocked(false);
          setError('Không thể kết nối đến máy chủ backend (http://localhost:5000). Vui lòng thử lại sau.');
        } else {
          setIsLocked(false);
          setError(data?.message || 'Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.');
        }
      } else {
        setIsLocked(false);
        setError('Đã xảy ra lỗi không xác định. Vui lòng thử lại.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="login-container">
      {/* Left Column: Form */}
      <section className="login-left-pane">
        <div className="login-form-wrapper">
          <div className="brand-tag">MERA · SOUVENIR</div>

          <h1 className="login-heading">
            <span>Quản lý Cửa hàng</span>
            <span>Đồ lưu niệm</span>
          </h1>

          <p className="login-subtitle">
            Chào mừng trở lại. Đăng nhập để bắt đầu ca làm việc.
          </p>

          <form className="login-form" onSubmit={handleSubmit} noValidate>
            {/* Field 1: Username */}
            <div className="form-group">
              <label htmlFor="username-input" className="form-label">
                Tên đăng nhập
              </label>
              <div className="input-container">
                <input
                  id="username-input"
                  type="text"
                  className="form-input"
                  placeholder="tranthuha"
                  value={tenDangNhap}
                  onChange={(e) => {
                    setTenDangNhap(e.target.value);
                    if (error) setError(null);
                  }}
                  autoComplete="username"
                  disabled={isSubmitting}
                />
              </div>
            </div>

            {/* Field 2: Password */}
            <div className="form-group">
              <label htmlFor="password-input" className="form-label">
                Mật khẩu
              </label>
              <div className="input-container">
                <input
                  id="password-input"
                  type={showPassword ? 'text' : 'password'}
                  className="form-input"
                  placeholder="••••••••"
                  value={matKhau}
                  onChange={(e) => {
                    setMatKhau(e.target.value);
                    if (error) setError(null);
                  }}
                  autoComplete="current-password"
                  disabled={isSubmitting}
                />
                <button
                  type="button"
                  className="password-toggle-btn"
                  onClick={() => setShowPassword(!showPassword)}
                  aria-label={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                >
                  {showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                </button>
              </div>
            </div>

            {/* Alert Box: Locked Account */}
            {isLocked && (
              <div className="alert-box" role="alert">
                <span className="alert-title">
                  Tài khoản tạm khóa sau 5 lần nhập sai
                </span>
                <span className="alert-subtitle">
                  Vui lòng liên hệ quản lý để được mở khóa.
                </span>
              </div>
            )}

            {/* General Error Banner */}
            {!isLocked && error && (
              <div className="error-banner" role="alert">
                {error}
              </div>
            )}

            {/* Success Banner */}
            {successMessage && (
              <div className="success-banner" role="status">
                {successMessage}
              </div>
            )}

            {/* Submit Button */}
            <div className="button-row">
              <button
                type="submit"
                className="login-submit-btn"
                disabled={isSubmitting}
              >
                {isSubmitting && <div className="btn-spinner" />}
                <span>{isSubmitting ? 'Đang đăng nhập...' : 'Đăng nhập'}</span>
              </button>
            </div>

            {/* Helper Note */}
            <p className="helper-text">
              Dùng tài khoản do quản lý cửa hàng cấp.
            </p>
          </form>
        </div>
      </section>

      {/* Right Column: Hero Showcase */}
      <section className="login-right-pane">
        <h2 className="hero-tagline">
          <span>Mỗi món quà,</span>
          <span>một kỷ niệm đẹp.</span>
        </h2>

        <div className="showcase-container">
          <div className="showcase-card">
            <img
              src="/gift-showcase.png"
              alt="MERA Souvenir Gift Showcase"
              className="showcase-image"
              onError={(e) => {
                // Fallback to figma-login-ref.png if gift-showcase isn't found
                (e.target as HTMLImageElement).src = '/figma-login-ref.png';
              }}
            />
          </div>
        </div>

        <div className="hero-footer">
          <div className="footer-highlight">
            Bán hàng nhanh · Quản lý kho · Kết nối khách hàng
          </div>
          <div className="footer-subtitle">
            Cửa hàng 01 · Hệ thống quản lý nội bộ
          </div>
        </div>
      </section>
    </div>
  );
};
