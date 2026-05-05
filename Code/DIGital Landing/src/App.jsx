import { useState, useRef, useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import './App.css'
import Game from './Game'
const digitalLogo = '/DIGitalLogo.png'
const nauLogo = '/NAULogo.png'

const LANGUAGES = [
  { code: 'en', label: 'English', flag: '🇺🇸' },
  { code: 'es', label: 'Español', flag: '🇪🇸' },
  { code: 'de', label: 'Deutsch', flag: '🇩🇪' },
]

export function LanguagePicker() {
  const { i18n } = useTranslation()
  const [open, setOpen] = useState(false)
  const ref = useRef(null)

  const current = LANGUAGES.find(l => l.code === i18n.language) || LANGUAGES[0]

  useEffect(() => {
    const handler = (e) => {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  return (
    <div className="lang-picker" ref={ref}>
      <button
        className="lang-picker-btn"
        onClick={() => setOpen(o => !o)}
        aria-haspopup="listbox"
        aria-expanded={open}
      >
        <span className="lang-flag">{current.flag}</span>
        <span className="lang-label">{current.label}</span>
        <span className={`lang-caret ${open ? 'open' : ''}`}>▾</span>
      </button>
      {open && (
        <ul className="lang-dropdown" role="listbox">
          {LANGUAGES.map(lang => (
            <li
              key={lang.code}
              role="option"
              aria-selected={lang.code === i18n.language}
              className={`lang-option ${lang.code === i18n.language ? 'active' : ''}`}
              onClick={() => { i18n.changeLanguage(lang.code); setOpen(false) }}
            >
              <span className="lang-flag">{lang.flag}</span>
              <span>{lang.label}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

function LandingPage() {
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const { t } = useTranslation()

  return (
    <div className="landing-page">
      <header className="navbar">
        <div className="container">
          <div className="logo">
            <img src={digitalLogo} alt="DIGital Logo" className="logo-image" />
          </div>
          <button className="menu-toggle" onClick={() => setIsMenuOpen(!isMenuOpen)}>☰</button>
          <nav className={isMenuOpen ? 'open' : ''}>
            <a href="#about">{t('nav.about')}</a>
            <Link to="/game">{t('nav.virtualExperience')}</Link>
            <a href="#quizzes">{t('nav.learningTools')}</a>
            <a href="#dr-sharp">{t('nav.meetDrSharp')}</a>
            <a href="#team">{t('nav.meetTeam')}</a>
            <LanguagePicker />
          </nav>
        </div>
      </header>

      <section className="hero">
        <div className="container">
          <div className="hero-content">
            <h1 className="hero-title">{t('hero.title')}</h1>
            <p className="hero-subtitle">{t('hero.subtitle')}</p>
            <div className="hero-buttons">
              <Link to="/game" className="btn btn-primary">{t('hero.exploreNow')}</Link>
              <a href="#about" className="btn btn-secondary">{t('hero.learnMore')}</a>
            </div>
          </div>
          <div className="hero-visual">
            <img src={digitalLogo} alt="DIGital Logo" className="hero-logo" />
          </div>
        </div>
      </section>

      <section id="about" className="about">
        <div className="container">
          <h2>{t('about.heading')}</h2>
          <p className="section-subtitle">{t('about.subtitle')}</p>
          <div className="about-content">
            <div className="about-text">
              <p>{t('about.p1')}</p>
              <p>{t('about.p2')}</p>
            </div>
            <div className="about-stats">
              <div className="stat"><h3>∞</h3><p>{t('about.stat1')}</p></div>
              <div className="stat"><h3>↓</h3><p>{t('about.stat2')}</p></div>
              <div className="stat"><h3>✓</h3><p>{t('about.stat3')}</p></div>
            </div>
          </div>
        </div>
      </section>

      <section id="features" className="features">
        <div className="container">
          <h2>{t('features.heading')}</h2>
          <p className="section-subtitle">{t('features.subtitle')}</p>
          <div className="feature-list">

            <div className="feature-row">
              <div className="feature-visual">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5"><rect x="3" y="3" width="18" height="14" rx="2"/><path d="M8 21h8M12 17v4"/></svg>
              </div>
              <div className="feature-text"><h3>{t('features.f1Title')}</h3><p>{t('features.f1Desc')}</p></div>
            </div>

            <div className="feature-row flip">
              <div className="feature-visual">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
              </div>
              <div className="feature-text"><h3>{t('features.f2Title')}</h3><p>{t('features.f2Desc')}</p></div>
            </div>

            <div className="feature-row">
              <div className="feature-visual">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5"><path d="M9 11l3 3L22 4"/><path d="M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11"/></svg>
              </div>
              <div className="feature-text"><h3>{t('features.f3Title')}</h3><p>{t('features.f3Desc')}</p></div>
            </div>

            <div className="feature-row flip">
              <div className="feature-visual">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5"><circle cx="12" cy="12" r="3"/><path d="M12 2v3M12 19v3M4.22 4.22l2.12 2.12M17.66 17.66l2.12 2.12"/></svg>
              </div>
              <div className="feature-text"><h3>{t('features.f4Title')}</h3><p>{t('features.f4Desc')}</p></div>
            </div>

            <div className="feature-row">
              <div className="feature-visual">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5"><path d="M3 3h18M3 12h18M3 21h18"/></svg>
              </div>
              <div className="feature-text"><h3>{t('features.f5Title')}</h3><p>{t('features.f5Desc')}</p></div>
            </div>

          </div>
        </div>
      </section>

      <section id="technology" className="technology">
        <div className="container">
          <h2>{t('technology.heading')}</h2>
          <div className="tech-grid">
            <div className="tech-card"><h3>{t('technology.t1Title')}</h3><p>{t('technology.t1Desc')}</p></div>
            <div className="tech-card"><h3>{t('technology.t2Title')}</h3><p>{t('technology.t2Desc')}</p></div>
            <div className="tech-card"><h3>{t('technology.t3Title')}</h3><p>{t('technology.t3Desc')}</p></div>
          </div>
        </div>
      </section>

      <section id="dr-sharp" className="dr-sharp">
        <div className="container">
          <h2>{t('drSharp.heading')}</h2>
          <p className="section-subtitle">{t('drSharp.subtitle')}</p>
          <div className="dr-sharp-content">
            <div className="dr-sharp-text">
              <ul className="dr-sharp-list">
                <li>{t('drSharp.fact1')}</li>
                <li>{t('drSharp.fact2')}</li>
                <li>{t('drSharp.fact3')}</li>
              </ul>
            </div>
            <div className="dr-sharp-photo">
              <div className="dr-sharp-placeholder">
                <span>📷</span>
                <p>Photo Coming Soon</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section id="team" className="team">
        <div className="container">
          <h2>{t('team.heading')}</h2>
          <p className="section-subtitle">{t('team.subtitle')}</p>
          <div className="team-grid">
            <div className="team-member">
              <img src="/Devin.JPEG" alt="Devin Jay San Nicolas" className="member-photo" />
              <h3>Devin Jay San Nicolas</h3>
              <p>{t('team.dj')}</p>
            </div>
            <div className="team-member">
              <img src="/Ryan.jpg" alt="Ryan Wood" className="member-photo" />
              <h3>Ryan Wood</h3>
              <p>{t('team.rw')}</p>
            </div>
            <div className="team-member">
              <img src="/Tate.jpg" alt="Tate Whittaker" className="member-photo" />
              <h3>Tate Whittaker</h3>
              <p>{t('team.tw')}</p>
            </div>
            <div className="team-member">
              <img src="/Jarom.jpg" alt="Jarom Craghead" className="member-photo" />
              <h3>Jarom Craghead</h3>
              <p>{t('team.jc')}</p>
            </div>
          </div>
          <div className="client-section">
            <h3>{t('team.collab')}</h3>
            <p className="client-name">Dr. Kayleigh Sharp</p>
            <p className="client-title">{t('team.drSharpTitle')}</p>
          </div>
        </div>
      </section>

      <section id="contact" className="cta-section">
        <div className="container">
          <h2>{t('cta.heading')}</h2>
          <p>{t('cta.body')}</p>
          <Link to="/game" className="btn btn-primary btn-large">{t('cta.btn')}</Link>
        </div>
      </section>

      <footer className="footer">
        <div className="container">
          <div className="footer-content">
            <div className="footer-section"><h4>{t('footer.title')}</h4></div>
            <div className="footer-section">
              <h4>{t('footer.quickLinks')}</h4>
              <ul>
                <li><a href="#about">{t('footer.about')}</a></li>
                <li><Link to="/game">{t('footer.playGame')}</Link></li>
                <li><a href="#quizzes">{t('footer.quizzes')}</a></li>
              </ul>
            </div>
            <div className="footer-section">
              <h4>{t('footer.connect')}</h4>
              <ul>
                <li><a href="https://github.com/Devin-Jay/DIGital" target="_blank" rel="noopener noreferrer">GitHub</a></li>
                <li><a href="https://sce.nau.edu/capstone/projects/CS/2026/DIGital_F25/" target="_blank" rel="noopener noreferrer">Team Website</a></li>
              </ul>
            </div>
            <div className="footer-section nau-section">
              <img src={nauLogo} alt="Northern Arizona University" className="nau-logo" />
            </div>
          </div>
          <div className="footer-bottom"><p>{t('footer.copyright')}</p></div>
        </div>
      </footer>
    </div>
  )
}

export default function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/game" element={<Game />} />
      </Routes>
    </Router>
  )
}