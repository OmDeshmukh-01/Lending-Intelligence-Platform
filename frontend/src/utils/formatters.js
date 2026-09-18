/**
 * Formatting utilities used across the frontend.
 * Centralised here so currency, LTV, and date formats are consistent.
 */

/** Format a number as GBP currency. */
export function formatCurrency(amount) {
  if (amount == null) return "—";
  return new Intl.NumberFormat("en-GB", {
    style: "currency",
    currency: "GBP",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount);
}

/** Format an LTV percentage to 2 decimal places. */
export function formatLtv(ltv) {
  if (ltv == null) return "N/A";
  return `${Number(ltv).toFixed(2)}%`;
}

/** Format a UTC ISO date string into a readable local date and time. */
export function formatDate(isoString) {
  if (!isoString) return "—";
  return new Date(isoString).toLocaleString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

/** Format a UTC ISO date string into a short local date. */
export function formatShortDate(isoString) {
  if (!isoString) return "—";
  return new Date(isoString).toLocaleDateString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}
