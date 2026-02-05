/**
 * Simulator Window Entry Point
 */
import React from 'react';
import ReactDOM from 'react-dom/client';
import { SimulatorApp } from './SimulatorApp';
import '../styles/index.css';
import './styles/stationpedia-game.css';

const root = ReactDOM.createRoot(document.getElementById('simulator-root') as HTMLElement);
root.render(
  <React.StrictMode>
    <SimulatorApp />
  </React.StrictMode>
);
