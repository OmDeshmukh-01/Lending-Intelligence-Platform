import { useNavigate } from "react-router-dom";
import DecisionBadge from "./DecisionBadge";
import { formatCurrency, formatLtv, formatShortDate } from "../utils/formatters";

export default function RecentApplications({ applications }) {
  const navigate = useNavigate();

  if (!applications || applications.length === 0) {
    return (
      <p className="text-sm text-slate-500 py-4 text-center">No applications yet.</p>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[520px]" role="grid">
        <thead>
          <tr className="border-b border-slate-200">
            {["Date", "Amount", "LTV", "Score", "Decision"].map(h => (
              <th key={h} className="px-3 py-2.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wider">
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {applications.map(app => (
            <tr
              key={app.id}
              className="hover:bg-slate-50 cursor-pointer transition-colors"
              onClick={() => navigate(`/applications/${app.id}`)}
              tabIndex={0}
              onKeyDown={e => e.key === "Enter" && navigate(`/applications/${app.id}`)}
              aria-label={`${app.decision} application for ${formatCurrency(app.loanAmount)}`}
            >
              <td className="px-3 py-2.5 text-sm text-slate-600 whitespace-nowrap">
                {formatShortDate(app.createdAt)}
              </td>
              <td className="px-3 py-2.5 text-sm font-medium text-slate-900 whitespace-nowrap">
                {formatCurrency(app.loanAmount)}
              </td>
              <td className="px-3 py-2.5 text-sm text-slate-600 whitespace-nowrap">
                {formatLtv(app.ltv)}
              </td>
              <td className="px-3 py-2.5 text-sm text-slate-600 whitespace-nowrap">
                {app.creditScore}
              </td>
              <td className="px-3 py-2.5">
                <DecisionBadge decision={app.decision} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
