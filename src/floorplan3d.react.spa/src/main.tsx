import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { initializeEvergine } from './evergine/evergine-initialize';

// Initialize Evergine
Blazor.start().then(() => initializeEvergine());

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
