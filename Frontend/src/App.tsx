// src/App.tsx
import { useState, useEffect } from 'react';
import reactLogo from './assets/react.svg';
import viteLogo from '/vite.svg';
import './App.css';

import { fetchImages, type ImageQueryParams } from './services/apiClient';   // <-- import the client

function App() {
  const [count, setCount] = useState(0);

  /* ------------------------------------------------------------------ */
  /* Demo: call the API once when the component mounts and log it      */
  /* ------------------------------------------------------------------ */
  useEffect(() => {
    const params: ImageQueryParams = {
      token: '3fa85f64-5717-4562-b3fc-2c963f66afa6', // replace with a real UUID
      page: 0,
      pageSize: 10,
    };

    fetchImages(params)
      .then((data) => console.log('✅ Images API response:', data))
      .catch((err) => console.error('❌ Failed to fetch images:', err));
  }, []);   // empty deps → run only once

  /* ------------------------------------------------------------------ */
  /* Rest of the component (unchanged)                                 */
  /* ------------------------------------------------------------------ */
  return (
    <>
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Vite + React</h1>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
    </>
  );
}

export default App;