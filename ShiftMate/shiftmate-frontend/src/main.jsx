// Importera nödvändiga bibliotek från React och React DOM.
import React from 'react';
import ReactDOM from 'react-dom/client';
// Importera BrowserRouter för att hantera routing i applikationen.
import { BrowserRouter } from 'react-router-dom';
// Importera huvud-App-komponenten.
import App from './App.jsx';
// Importera globala stilmallar, inklusive Tailwind CSS.
import './index.css';

// Renderar rotkomponenten i DOM.
// ReactDOM.createRoot är det nya sättet att initiera en React-app i version 18.
ReactDOM.createRoot(document.getElementById('root')).render(
  // React.StrictMode aktiverar ytterligare kontroller och varningar för att identifiera potentiella problem i applikationen.
  <React.StrictMode>
    {/* BrowserRouter omsluter hela applikationen för att möjliggöra routing. */}
    <BrowserRouter>
      {/* App-komponenten är startpunkten för hela applikationen. */}
      <App />
    </BrowserRouter>
  </React.StrictMode>,
);