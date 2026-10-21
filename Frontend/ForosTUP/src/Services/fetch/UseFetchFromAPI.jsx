import { useEffect, useState, useCallback } from "react";

export const useFetchFromAPI = (apiRoute, initialValue = []) => {
  const [data, setData] = useState(initialValue);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Memoizamos la función para que se mantenga estable entre re-renders
  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`http://localhost:3000${apiRoute}`);
      if (!response.ok) {
        throw new Error("No se pudo recuperar la información");
      }
      const json = await response.json();
      setData(json);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [apiRoute]);

  // Se ejecuta cuando se monta el componente o cuando cambia la ruta de la API
  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Agregamos una función refetch para recargar las páginas
  return { data, loading, error, refetch: fetchData };
};