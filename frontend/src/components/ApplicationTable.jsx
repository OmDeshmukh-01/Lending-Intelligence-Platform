import ApplicationRow from "./ApplicationRow";
import EmptyState from "./EmptyState";
import { useNavigate } from "react-router-dom";

export default function ApplicationTable({ applications }) {
  const navigate = useNavigate();

  if (!applications || applications.length === 0) {
    return (
      <EmptyState
        title="No applications found"
        description="No applications match your selected filters."
      />
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[640px]" role="grid">
        <thead>
          <tr className="border-b border-slate-200">
            {["Date", "Loan Amount", "Asset Value", "LTV", "Credit Score", "Decision", ""].map(h => (
              <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-slate-500 uppercase tracking-wider">
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {applications.map(app => (
            <ApplicationRow key={app.id} application={app} />
          ))}
        </tbody>
      </table>
    </div>
  );
}
