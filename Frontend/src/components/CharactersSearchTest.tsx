/* src/components/CharacterSearchTest.tsx */

import { useEffect, useState } from "react";
import type { GetCharacterResponsePaginatedResponse } from "../types/character";
import { searchCharacters } from "../api/characters";



const CharacterSearchTest: React.FC = () => {
  const [data, setData] = useState<GetCharacterResponsePaginatedResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const result = await searchCharacters({
          q: '',   
          page: 1,
          pageSize: 100, 
        });
        setData(result);
      } catch (err: any) {
        console.error(err);
        setError(err.message ?? 'Unexpected error');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  return (
    <div style={{ padding: '1rem', fontFamily: 'sans-serif' }}>
      <h2>Character Search Test</h2>

      {loading && <p>Loading…</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}

      {data && (
        <>
          <h3>Result (JSON)</h3>
          <pre
            style={{
              background: '#f5f5f5',
              padding: '1rem',
              overflowX: 'auto',
            }}
          >
            {JSON.stringify(data, null, 2)}
          </pre>

          {/* Optional – a quick table view */}
          <h3>Characters (Table)</h3>
          <table
            style={{
              borderCollapse: 'collapse',
              width: '100%',
              marginTop: '1rem',
            }}
          >
            <thead>
              <tr>
                <th style={{ border: '1px solid #ddd', padding: '0.5rem' }}>
                  Tag ID
                </th>
                <th style={{ border: '1px solid #ddd', padding: '0.5rem' }}>
                  Name
                </th>
                <th style={{ border: '1px solid #ddd', padding: '0.5rem' }}>
                  Count
                </th>
              </tr>
            </thead>
            <tbody>
              {data.data?.map((c) => (
                <tr key={c.tagId}>
                  <td style={{ border: '1px solid #ddd', padding: '0.5rem' }}>
                    {c.tagId}
                  </td>
                  <td style={{ border: '1px solid #ddd', padding: '0.5rem' }}>
                    {c.characterName ?? '(none)'}
                  </td>
                  <td style={{ border: '1px solid #ddd', padding: '0.5rem' }}>
                    {c.count}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
};

export default CharacterSearchTest;
