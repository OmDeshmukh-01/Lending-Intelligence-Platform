import { useState } from "react";
import { applyForLoan } from "../api/loanApi";
import DecisionBadge from "../components/DecisionBadge";
import RuleEvaluation from "../components/RuleEvaluation";
import { formatCurrency, formatLtv } from "../utils/formatters";
import { Link } from "react-router-dom";

const INITIAL_FORM = { loanAmount: "", assetValue: "", creditScore: "" };
const INITIAL_ERRORS = {};

function validate(form) {
  const errors = {};
  const loan = parseFloat(form.loanAmount);
  const asset = parseFloat(form.assetValue);
  const score = parseInt(form.creditScore, 10);

  if (!form.loanAmount || isNaN(loan) || loan <= 0)
    errors.loanAmount = "Loan amount must be greater than £0.";
  if (!form.assetValue || isNaN(asset) || asset <= 0)
    errors.assetValue = "Asset value must be greater than £0.";
  if (!form.creditScore || isNaN(score) || score < 1 || score > 999 || !Number.isInteger(score))
    errors.creditScore = "Credit score must be an integer between 1 and 999.";

  return errors;
}

export default function NewApplication() {
  const [form, setForm] = useState(INITIAL_FORM);
  const [fieldErrors, setFieldErrors] = useState(INITIAL_ERRORS);
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState(null);
  const [apiError, setApiError] = useState(null);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(f => ({ ...f, [name]: value }));
    // Clear field error on change
    if (fieldErrors[name]) setFieldErrors(fe => ({ ...fe, [name]: undefined }));
    setApiError(null);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setResult(null);
    setApiError(null);

    // Client-side validation
    const errors = validate(form);
    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    setSubmitting(true);
    try {
      const res = await applyForLoan({
        loanAmount: parseFloat(form.loanAmount),
        assetValue: parseFloat(form.assetValue),
        creditScore: parseInt(form.creditScore, 10),
      });
      setResult(res);
      setForm(INITIAL_FORM);
      setFieldErrors(INITIAL_ERRORS);
    } catch (err) {
      if (err.status === 400 && err.data?.errors) {
        // Map backend validation errors to field errors
        const mapped = {};
        for (const [key, msgs] of Object.entries(err.data.errors)) {
          mapped[key] = msgs[0];
        }
        setFieldErrors(mapped);
      } else {
        setApiError(err.message || "An unexpected error occurred. Please try again.");
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleReset = () => {
    setResult(null);
    setApiError(null);
    setFieldErrors(INITIAL_ERRORS);
  };

  return (
    <main className="flex-1 p-4 sm:p-6 lg:p-12 overflow-auto flex flex-col items-center justify-start min-h-full">
      <div className="w-full max-w-4xl">
        <header className="mb-8 text-center">
          <h1 className="text-2xl font-bold text-slate-900">New Loan Application</h1>
          <p className="text-sm text-slate-500 mt-2">
            Submit a loan application for evaluation against our lending criteria.
          </p>
        </header>

        <div className="w-full space-y-6">
        {/* Application Form */}
        {!result && (
          <form onSubmit={handleSubmit} noValidate className="card p-6 space-y-5">
            <h2 className="text-base font-semibold text-slate-900">Application Details</h2>

            {/* Loan Amount */}
            <div>
              <label htmlFor="loanAmount" className="form-label">
                Loan Amount (GBP) <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 font-medium">£</span>
                <input
                  id="loanAmount"
                  name="loanAmount"
                  type="number"
                  min="0"
                  step="1000"
                  value={form.loanAmount}
                  onChange={handleChange}
                  placeholder="500000"
                  className={`form-input pl-7 ${fieldErrors.loanAmount ? "form-input-error" : ""}`}
                  aria-describedby={fieldErrors.loanAmount ? "loanAmount-error" : undefined}
                  aria-invalid={!!fieldErrors.loanAmount}
                  disabled={submitting}
                />
              </div>
              {fieldErrors.loanAmount && (
                <p id="loanAmount-error" className="form-error" role="alert">{fieldErrors.loanAmount}</p>
              )}
              <p className="text-xs text-slate-400 mt-1">Permitted range: £100,000 – £1,500,000</p>
            </div>

            {/* Asset Value */}
            <div>
              <label htmlFor="assetValue" className="form-label">
                Asset Value (GBP) <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 font-medium">£</span>
                <input
                  id="assetValue"
                  name="assetValue"
                  type="number"
                  min="0"
                  step="1000"
                  value={form.assetValue}
                  onChange={handleChange}
                  placeholder="1000000"
                  className={`form-input pl-7 ${fieldErrors.assetValue ? "form-input-error" : ""}`}
                  aria-describedby={fieldErrors.assetValue ? "assetValue-error" : undefined}
                  aria-invalid={!!fieldErrors.assetValue}
                  disabled={submitting}
                />
              </div>
              {fieldErrors.assetValue && (
                <p id="assetValue-error" className="form-error" role="alert">{fieldErrors.assetValue}</p>
              )}
              <p className="text-xs text-slate-400 mt-1">The value of the asset securing the loan</p>
            </div>

            {/* Credit Score */}
            <div>
              <label htmlFor="creditScore" className="form-label">
                Credit Score <span className="text-red-500">*</span>
              </label>
              <input
                id="creditScore"
                name="creditScore"
                type="number"
                min="1"
                max="999"
                step="1"
                value={form.creditScore}
                onChange={handleChange}
                placeholder="750"
                className={`form-input ${fieldErrors.creditScore ? "form-input-error" : ""}`}
                aria-describedby={fieldErrors.creditScore ? "creditScore-error" : undefined}
                aria-invalid={!!fieldErrors.creditScore}
                disabled={submitting}
              />
              {fieldErrors.creditScore && (
                <p id="creditScore-error" className="form-error" role="alert">{fieldErrors.creditScore}</p>
              )}
              <p className="text-xs text-slate-400 mt-1">Integer between 1 and 999</p>
            </div>

            {/* API error */}
            {apiError && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3" role="alert">
                <p className="text-sm text-red-700">{apiError}</p>
              </div>
            )}

            <button
              type="submit"
              className="btn-primary w-full py-2.5"
              disabled={submitting}
              aria-busy={submitting}
            >
              {submitting ? (
                <span className="flex items-center justify-center gap-2">
                  <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                  </svg>
                  Evaluating Application…
                </span>
              ) : "Proceed — Evaluate Application"}
            </button>
          </form>
        )}

        {/* Decision Result */}
        {result && (
          <div className="space-y-4">
            {/* Decision banner */}
            <div className={`card p-6 ${result.decision === "Approved" ? "border-emerald-200 bg-emerald-50/50" : "border-red-200 bg-red-50/50"}`}>
              <div className="flex items-start justify-between gap-4 flex-wrap">
                <div>
                  <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">Decision</p>
                  <DecisionBadge decision={result.decision} size="large" />
                </div>
                <div className="text-right">
                  <p className="text-xs text-slate-500">Application ID</p>
                  <p className="text-xs font-mono text-slate-700 break-all">{result.id}</p>
                </div>
              </div>
              {result.decisionReason && (
                <p className="text-sm text-slate-700 mt-4 border-t border-slate-200/70 pt-4">
                  {result.decisionReason}
                </p>
              )}
            </div>

            {/* Application summary */}
            <div className="card p-6">
              <h2 className="text-base font-semibold text-slate-900 mb-4">Application Summary</h2>
              <dl className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                {[
                  { label: "Loan Amount", value: formatCurrency(result.loanAmount) },
                  { label: "Asset Value", value: formatCurrency(result.assetValue) },
                  { label: "Credit Score", value: result.creditScore },
                  { label: "LTV", value: formatLtv(result.ltv) },
                ].map(({ label, value }) => (
                  <div key={label}>
                    <dt className="text-xs text-slate-500">{label}</dt>
                    <dd className="text-sm font-semibold text-slate-900 mt-0.5">{value}</dd>
                  </div>
                ))}
              </dl>
            </div>

            {/* Rule evaluations */}
            <div className="card p-6">
              <RuleEvaluation rules={result.rules} />
            </div>

            {/* Actions */}
            <div className="flex flex-wrap gap-3">
              <button onClick={handleReset} className="btn-primary">
                New Application
              </button>
              <Link to={`/applications/${result.id}`} className="btn-secondary">
                View Full Details
              </Link>
              <Link to="/applications" className="btn-secondary">
                All Applications
              </Link>
            </div>
          </div>
        )}
      </div>
      </div>
    </main>
  );
}
