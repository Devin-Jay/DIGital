import { useEffect, useRef, useState } from 'react'
import './Game.css'

export default function Game() 
{
  const canvasRef = useRef(null)
  const [isLoading, setIsLoading] = useState(true)
  const [loadingProgress, setLoadingProgress] = useState(0)
  const [isFullscreen, setIsFullscreen] = useState(false)
  const [unityInstance, setUnityInstance] = useState(null)

  useEffect(() => 
  {
    // Load Unity WebGL build
    const loadUnityGame = async () => 
    {
      try 
      {
        const buildUrl = '/DemoBuild'
        const config = 
        {
          dataUrl: `${buildUrl}/WebGL.data.br`,
          frameworkUrl: `${buildUrl}/WebGL.framework.js.br`,
          codeUrl: `${buildUrl}/WebGL.wasm.br`,
          streamingAssetsUrl: 'StreamingAssets',
          companyName: 'DIGital',
          productName: 'Virtual Excavation',
          productVersion: '1.0',
        }

        // Load Unity loader script
        if (!window.createUnityInstance) 
        {
          const script = document.createElement('script')
          script.src = `${buildUrl}/WebGL.loader.js`
          script.async = true
          document.body.appendChild(script)
          
          await new Promise((resolve) => 
          {
            script.onload = resolve
          })
        }

        // Create Unity instance
        const instance = await window.createUnityInstance(canvasRef.current, config, (progress) => {
          setLoadingProgress(Math.round(progress * 100))
        })

        setUnityInstance(instance)
        setIsLoading(false)
      } catch (error) {
        console.error('Error loading Unity game:', error)
        setIsLoading(false)
      }
    }

    loadUnityGame()

    // Cleanup
    return () => {
      if (unityInstance) {
        unityInstance.Quit()
      }
    }
  }, [])

  const toggleFullscreen = () => {
    if (!isFullscreen) {
      unityInstance?.SetFullscreen(1)
      setIsFullscreen(true)
    } else {
      unityInstance?.SetFullscreen(0)
      setIsFullscreen(false)
    }
  }

  return (
    <div className="game-page">
      {/* Navigation Bar */}
      <nav className="game-nav">
        <a href="/" className="back-link">← Back to Home</a>
        <h1>DIGital Virtual Excavation</h1>
        <div className="nav-controls">
          <button onClick={toggleFullscreen} className="fullscreen-btn">
            {isFullscreen ? '⊡ Exit Fullscreen' : '⛶ Fullscreen'}
          </button>
        </div>
      </nav>

      {/* Game Container */}
      <div className="game-container">
        <div className="game-wrapper">
          {isLoading && (
            <div className="loading-overlay">
              <div className="loading-content">
                <div className="spinner"></div>
                <h2>Loading Virtual Excavation...</h2>
                <div className="progress-bar">
                  <div 
                    className="progress-fill" 
                    style={{ width: `${loadingProgress}%` }}
                  ></div>
                </div>
                <p>{loadingProgress}%</p>
              </div>
            </div>
          )}
          
          <canvas 
            ref={canvasRef} 
            id="unity-canvas"
            className={isLoading ? 'hidden' : ''}
          ></canvas>
        </div>

        {/* Game Info Sidebar */}
        <div className="game-info">
          <div className="info-section">
            <h3>🎮 Controls</h3>
            <ul>
              <li><strong>WASD</strong> - Move around</li>
              <li><strong>Mouse</strong> - Look around</li>
            </ul>
          </div>

          <div className="info-section">
            <h3>📚 Objectives</h3>
            <ul>
              <li>Explore the archaeological site</li>
              <li>Identify and document artifacts</li>
              <li>Learn proper excavation techniques</li>
              <li>Complete The Archaeology Process!</li>
            </ul>
          </div>

          <div className="info-section">
            <h3>💡 Tips</h3>
            <ul>
              <li>Take your time examining each area</li>
              <li>Use the AI assistant for guidance</li>
              <li>Document everything you find</li>
            </ul>
          </div>

        </div>
      </div>

      {/* Footer */}
      <footer className="game-footer">
        <p>
          Need help? Check out the <a href="/tutorial">Tutorial</a>
        </p>
      </footer>
    </div>
  )
}
