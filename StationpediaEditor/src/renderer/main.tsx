import React from 'react';
import ReactDOM from 'react-dom/client';
import './styles/index.css';
import 'react-mosaic-component/react-mosaic-component.css';
import './editor/styles/editor.css';
import './simulator/styles/stationpedia.css';
import { EditorApp } from './editor/EditorApp';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <EditorApp />
  </React.StrictMode>
);
