import { useParams, Link, useNavigate } from "react-router-dom";
import { useApplication } from "../hooks/useApplication";
import DecisionBadge from "../components/DecisionBadge";
import RuleEvaluation from "../components/RuleEvaluation";
import LoadingState from "../components/LoadingState";
import ErrorState from "../components/ErrorState";
import { formatCurrency, formatLtv, formatDate } from "../utils/formatters";

export default function ApplicationDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data, loading, error } = useApplication(id);

  return (
    <main className="flex-1 p-4 sm:p-6 lg:p-8 overflow-auto">
      <div className="mb-5">
        <button
          onClick={() => navigate(-1)}
          className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-900 transition-colors"
          aria-label="Go back"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          Back
        </button>
      </div>

      {loading && <LoadingState message="Loading application..." />}
      {error && <ErrorState message={error} />}

      {!loading && !error && data && (
        <div className="max-w-2xl space-y-5">
          {/* Header card */}
          <div className={`card p-6 ${data.decision === "Approved" ? "border-emerald-200 bg-emerald-50/40" : "border-red-200 bg-red-50/40"}`}>
            <div className="flex items-start justify-between gap-4 flex-wrap">
              <div>
                <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">Decision</p>
                <DecisionBadge decision={data.decision} size="large" />
              </div>
              <div className="text-sm text-slate-500 text-right">
                <p className="font-mono text-xs text-slate-400 mb-0.5">ID: {data.id}</p>
                <p>{formatDate(data.createdAt)}</p>
              </div>
            </div>
            {data.decisionReason && (
              <p className="text-sm text-slate-700 mt-4 border-t border-slate-200/60 pt-4">
                {data.decisionReason}
              </p>
            )}
          </div>

          {/* Application data */}
          <div className="card p-6">
            <h2 className="text-base font-semibold text-slate-900 mb-4">Application Details</h2>
            <dl className="grid grid-cols-2 gap-x-6 gap-y-4">
              {[
                { label: "Loan Amount", value: formatCurrency(data.loanAmount) },
                { label: "Asset Value", value: formatCurrency(data.assetValue) },
                { label: "Credit Score", value: data.creditScore },
                { label: "Loan to Value (LTV)", value: formatLtv(data.ltv) },
                { label: "Date Submitted", value: formatDate(data.createdAt) },
              ].map(({ label, value }) => (
                <div key={label}>
                  <dt className="text-xs text-slate-500 font-medium">{label}</dt>
                  <dd className="text-sm font-semibold text-slate-900 mt-0.5">{value}</dd>
                </div>
              ))}
            </dl>
          </div>

          {/* Rule evaluations */}
          <div className="card p-6">
            <RuleEvaluation rules={data.rules} />
          </div>

          {/* Navigation */}
          <div className="flex gap-3 flex-wrap">
            <Link to="/applications/new" className="btn-primary">New Application</Link>
            <Link to="/applications" className="btn-secondary">All Applications</Link>
          </div>
        </div>
      )}
    </main>
  );
}
