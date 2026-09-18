import { useState, useEffect } from "react";
import { getApplicationById } from "../api/loanApi";

export function useApplication(id) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    setError(null);
    getApplicationById(id)
      .then(setData)
      .catch(err => {
        setError(err.status === 404 ? "Application not found." : (err.message || "Failed to load application."));
      })
      .finally(() => setLoading(false));
  }, [id]);

  return { data, loading, error };
}
