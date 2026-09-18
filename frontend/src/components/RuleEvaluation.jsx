/**
 * Displays the list of business rules evaluated for an application.
 * Each rule shows pass/fail with an icon AND text — never color alone.
 */
export default function RuleEvaluation({ rules }) {
  if (!rules || rules.length === 0) return null;

  return (
    <div>
      <h3 className="text-sm font-semibold text-slate-700 mb-3">Business Rule Evaluation</h3>
      <ul className="space-y-2" aria-label="Business rules">
        {rules.map((rule, i) => (
          <li key={i} className={`flex items-start gap-3 p-3 rounded-lg ${
            rule.passed ? "bg-emerald-50 border border-emerald-100" : "bg-red-50 border border-red-100"
          }`}>
            <span className={`flex-shrink-0 mt-0.5 ${rule.passed ? "text-emerald-600" : "text-red-600"}`}
                  aria-hidden="true">
              {rule.passed ? (
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
                </svg>
              ) : (
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M6 18L18 6M6 6l12 12" />
                </svg>
              )}
            </span>
            <div className="min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <span className={`text-xs font-bold uppercase tracking-wide ${rule.passed ? "text-emerald-700" : "text-red-700"}`}>
                  {rule.passed ? "Passed" : "Failed"}
                </span>
                <span className="text-sm font-medium text-slate-800">{rule.name}</span>
              </div>
              {rule.detail && (
                <p className="text-xs text-slate-600 mt-0.5">{rule.detail}</p>
              )}
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
