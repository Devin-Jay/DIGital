import { useState } from 'react'
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import './App.css'
import Game from './Game'
const digitalLogo = '/DIGitalLogo.png'
const nauLogo = '/NAULogo.png'

function LandingPage() 
{
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  return (
    <div className="landing-page">
      {/* Navigation */}
      <header className="navbar">
        <div className="container">
          <div className="logo">
            <img src={digitalLogo} alt="DIGital Logo" className="logo-image" />
          </div>
          <button className="menu-toggle" onClick={() => setIsMenuOpen(!isMenuOpen)}>
            ☰
          </button>
          <nav className={isMenuOpen ? 'open' : ''}>
            <a href="#about">About</a>
            <Link to="/game">Virtual Experience</Link>
            <a href="#quizzes">Learning Tools</a>
            <a href="#assistant">AI Assistant</a>
            <a href="#dr-sharp">Meet Dr. Sharp</a>
            <a href="#team">Meet The Team</a>
          </nav>
        </div>
      </header>

      {/* Hero Section */}
      <section className="hero">
        <div className="container">
          <div className="hero-content">
            <h1 className="hero-title">Discover Archaeology Virtually</h1>
            <p className="hero-subtitle">
              Explore virtual archaeological sites from *anywhere*. Experience immersive field school training without the cost or travel.
            </p>
            <div className="hero-buttons">
              <Link to="/game" className="btn btn-primary">Explore Now</Link>
              <a href="#about" className="btn btn-secondary">Learn More</a>
            </div>
          </div>
          <div className="hero-visual">
            <img src={digitalLogo} alt="DIGital Logo" className="hero-logo" />
          </div>
        </div>
      </section>

      {/* About Section */}
      <section id="about" className="about">
        <div className="container">
          <h2>What is DIGital?</h2>
          <p className="section-subtitle">
            Transforming Archaeological Education
          </p>
          <div className="about-content">
            <div className="about-text">
              <p>
                DIGital Virtual Excavation is a web-based platform designed to make archaeological field school training accessible, affordable, and engaging. Traditional field schools can cost thousands of dollars and require travel to remote sites. We're changing that.
              </p>
              <p>
                Our platform allows students and educators to explore authentic archaeological sites in immersive virtual environments, replacing expensive traditional programs while maintaining educational integrity.
              </p>
            </div>
            <div className="about-stats">
              <div className="stat">
                <h3>∞</h3>
                <p>Accessible Anywhere</p>
              </div>
              <div className="stat">
                <h3>↓</h3>
                <p>Will always be free</p>
              </div>
              <div className="stat">
                <h3>✓</h3>
                <p>Same Education Quality</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="features">
        <div className="container">
          <h2>Key Features</h2>
          <p className="section-subtitle">
            Everything you need for immersive archaeological education
          </p>
          <div className="feature-grid">
            <div className="feature-card">
              <h3>Digital Archaeological Experiences</h3>
              <p>Access immersive experiences directly from your browser.</p>
            </div>
            <div className="feature-card">
              <h3>Authentic Sites</h3>
              <p>Explore meticulously recreated archaeological sites based on real excavations and research.</p>
            </div>
            <div className="feature-card">
              <h3>Learning Outcomes</h3>
              <p>Monitor learning outcomes with built-in assessment tools and detailed progress reports.</p>
            </div>
            <div className="feature-card">
              <h3>AI-Powered Guidance</h3>
              <p>Get intelligent assistance and contextual information about artifacts and excavation techniques.</p>
            </div>
            <div className="feature-card">
              <h3>Understand the Process</h3>
              <p>Experience all steps of archaeological process</p>
            </div>
          </div>
        </div>
      </section>

      {/* Technology Section */}
      <section id="technology" className="technology">
        <div className="container">
          <h2>DIGital is committed to bring a high-quality product</h2>
          <div className="tech-grid">
            <div className="tech-card">
              <h3>Unity WebGL</h3>
              <p>High-performance 3D graphics rendered directly in the browser for immersive experiences.</p>
            </div>
            <div className="tech-card">
              <h3>React & Next.js</h3>
              <p>Modern frontend framework for a responsive, fast, and intuitive user interface.</p>
            </div>
            <div className="tech-card">
              <h3>AI Integration</h3>
              <p>Intelligent assistant to provide contextual guidance and enhance the learning experience.</p>
            </div>
          </div>
        </div>
      </section>

      {/* Team Section */}
      <section id="team" className="team">
        <div className="container">
          <h2>Our Team</h2>
          <p className="section-subtitle">
            Computer Science students at Northern Arizona University
          </p>
          <div className="team-grid">
            <div className="team-member">
              <div className="member-avatar">DJ</div>
              <h3>Devin Jay San Nicolas</h3>
              <p>Team Lead</p>
            </div>
            <div className="team-member">
              <div className="member-avatar">RW</div>
              <h3>Ryan Wood</h3>
              <p>Frontend Development</p>
            </div>
            <div className="team-member">
              <div className="member-avatar">TW</div>
              <h3>Tate Whittaker</h3>
              <p>Game Development</p>
            </div>
            <div className="team-member">
              <div className="member-avatar">JC</div>
              <h3>Jarom Craghead</h3>
              <p>Backend & Integration</p>
            </div>
          </div>
          <div className="client-section">
            <h3>In Collaboration With</h3>
            <p className="client-name">Dr. Kayleigh Sharp</p>
            <p className="client-title">Project Client & Archaeological Advisor</p>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section id="contact" className="cta-section">
        <div className="container">
          <h2>Ready to Explore?</h2>
          <p>
            Join us in revolutionizing archaeological education.
          </p>
          <Link to="/game" className="btn btn-primary btn-large">Start Your Excavation</Link>
        </div>
      </section>

      {/* Footer */}
      <footer className="footer">
        <div className="container">
          <div className="footer-content">
            <div className="footer-section">
              <h4>DIGital</h4>
              <p>A Virtual Excavation Experience</p>
            </div>
            <div className="footer-section">
              <h4>Quick Links</h4>
              <ul>
                <li><a href="#about">About</a></li>
                <li><Link to="/game">Play Game</Link></li>
                <li><a href="#quizzes">Quizzes</a></li>
                <li><a href="#assistant">Assistant</a></li>
              </ul>
            </div>
            <div className="footer-section">
              <h4>Connect</h4>
              <ul>
                <li>
                  <a 
                    href="https://github.com/Devin-Jay/DIGital" 
                    target="_blank" 
                    rel="noopener noreferrer"
                  >
                  GitHub
                  </a>
                </li>
                <li>
                  <a 
                    href="https://sce.nau.edu/capstone/projects/CS/2026/DIGital_F25/" 
                    target="_blank" 
                    rel="noopener noreferrer"
                  >
                  Team Website
                  </a>
                </li>
                </ul>
            </div>
            <div className="footer-section nau-section">
              <img src={nauLogo} alt="Northern Arizona University" className="nau-logo" />
            </div>
          </div>
          <div className="footer-bottom">
            <p>&copy; 2025 DIGital Virtual Excavation</p>
          </div>
        </div>
      </footer>
    </div>
  )
}

export default function App() 
{
  return (
    <Router>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/game" element={<Game />} />
      </Routes>
    </Router>
  )
}
