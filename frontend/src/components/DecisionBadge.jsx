/**
 * Displays the application decision as a prominent, accessible status badge.
 * Never relies on color alone — always includes an icon and text label.
 */
export default function DecisionBadge({ decision, size = "default" }) {
  const isApproved = decision === "Approved";
  const sizeClasses = size === "large"
    ? "text-lg font-bold px-6 py-3 rounded-xl gap-3"
    : "text-sm font-semibold px-3 py-1.5 rounded-lg gap-1.5";

  return (
    <span
      className={`inline-flex items-center ${sizeClasses} ${
        isApproved
          ? "bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200"
          : "bg-red-50 text-red-700 ring-1 ring-red-200"
      }`}
      role="status"
      aria-label={`Decision: ${decision}`}
    >
      {isApproved ? (
        <svg className={size === "large" ? "w-6 h-6" : "w-4 h-4"} fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
        </svg>
      ) : (
        <svg className={size === "large" ? "w-6 h-6" : "w-4 h-4"} fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
        </svg>
      )}
      {decision?.toUpperCase()}
    </span>
  );
}
