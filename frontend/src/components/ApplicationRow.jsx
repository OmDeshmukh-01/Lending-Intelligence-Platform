import { useNavigate } from "react-router-dom";
import DecisionBadge from "./DecisionBadge";
import { formatCurrency, formatLtv, formatShortDate } from "../utils/formatters";

/** Single row in the applications table. */
export default function ApplicationRow({ application }) {
  const navigate = useNavigate();

  return (
    <tr
      className="hover:bg-slate-50 cursor-pointer transition-colors"
      onClick={() => navigate(`/applications/${application.id}`)}
      tabIndex={0}
      role="row"
      onKeyDown={e => e.key === "Enter" && navigate(`/applications/${application.id}`)}
      aria-label={`Application ${application.id}, ${application.decision}`}
    >
      <td className="px-4 py-3 text-sm text-slate-600 whitespace-nowrap">
        {formatShortDate(application.createdAt)}
      </td>
      <td className="px-4 py-3 text-sm font-medium text-slate-900 whitespace-nowrap">
        {formatCurrency(application.loanAmount)}
      </td>
      <td className="px-4 py-3 text-sm text-slate-600 whitespace-nowrap">
        {formatCurrency(application.assetValue)}
      </td>
      <td className="px-4 py-3 text-sm text-slate-600 whitespace-nowrap">
        {formatLtv(application.ltv)}
      </td>
      <td className="px-4 py-3 text-sm text-slate-600 whitespace-nowrap">
        {application.creditScore}
      </td>
      <td className="px-4 py-3 whitespace-nowrap">
        <DecisionBadge decision={application.decision} />
      </td>
      <td className="px-4 py-3">
        <span className="text-sm text-brand-600 font-medium hover:text-brand-700">View →</span>
      </td>
    </tr>
  );
}
