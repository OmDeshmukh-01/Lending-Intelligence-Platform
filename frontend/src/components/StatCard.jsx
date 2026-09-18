/**
 * A single KPI card on the dashboard.
 */
export default function StatCard({ title, value, subtitle, icon, colorClass = "text-brand-600" }) {
  return (
    <div className="card p-5">
      <div className="flex items-start justify-between">
        <div className="min-w-0">
          <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-1">{title}</p>
          <p className={`text-2xl font-bold ${colorClass} truncate`}>{value}</p>
          {subtitle && <p className="text-xs text-slate-400 mt-1">{subtitle}</p>}
        </div>
        {icon && (
          <div className={`flex-shrink-0 w-10 h-10 rounded-lg bg-slate-50 flex items-center justify-center ${colorClass}`}
               aria-hidden="true">
            {icon}
          </div>
        )}
      </div>
    </div>
  );
}
