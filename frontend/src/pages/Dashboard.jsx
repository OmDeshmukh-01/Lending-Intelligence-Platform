import { Link } from 'react-router-dom';
import { useDashboard } from "../hooks/useDashboard";
import StatCard from "../components/StatCard";
import RecentApplications from "../components/RecentApplications";
import LoadingState from "../components/LoadingState";
import ErrorState from "../components/ErrorState";
import { formatCurrency, formatLtv } from "../utils/formatters";
import {
  PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';

const icons = {
  total: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.75}
        d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
    </svg>
  ),
  approved: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.75} d="M5 13l4 4L19 7" />
    </svg>
  ),
  declined: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.75} d="M6 18L18 6M6 6l12 12" />
    </svg>
  ),
  loans: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.75} d="M18 7c0-5.333-8-5.333-8 0" />
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.75} d="M10 7v14" />
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.75} d="M6 21h12" />
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.75} d="M6 13h10" />
    </svg>
  ),
  ltv: (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.75}
        d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
    </svg>
  ),
};

const PIE_COLORS = ['#4f46e5', '#f43f5e']; // Brand-600 (Indigo) for approved, Rose-500 for declined // Emerald-600 for approved, Red-600 for declined

export default function Dashboard() {
  const { data, loading, error, refresh } = useDashboard();

  let pieData = [];
  if (data) {
    pieData = [
      { name: 'Approved', value: data.successfulApplications },
      { name: 'Declined', value: data.declinedApplications }
    ];
  }

  return (
    <main className="flex-1 p-4 sm:p-6 lg:p-8 overflow-auto">
      <header className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900">Lending Dashboard</h1>
        <p className="text-sm text-slate-500 mt-1">
          Overview of your lending portfolio and recent application activity.
        </p>
      </header>

      {loading && <LoadingState message="Loading dashboard..." />}
      {error && <ErrorState message={error} onRetry={refresh} />}

      {!loading && !error && data && (
        <>
          {/* KPI Stats */}
          <section aria-label="Portfolio statistics" className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4 mb-8">
            <StatCard
              title="Total Applications"
              value={data.totalApplications}
              icon={icons.total}
            />
            <StatCard
              title="Approved"
              value={data.successfulApplications}
              icon={icons.approved}
              colorClass="text-emerald-600"
            />
            <StatCard
              title="Declined"
              value={data.declinedApplications}
              icon={icons.declined}
              colorClass="text-red-600"
            />
            <StatCard
              title="Total Loans Written"
              value={formatCurrency(data.totalLoansWritten)}
              subtitle="Successful applications only"
              icon={icons.loans}
              colorClass="text-brand-600"
            />
            <StatCard
              title="Mean LTV"
              value={data.meanLtv != null ? formatLtv(data.meanLtv) : "N/A"}
              subtitle="All applications"
              icon={icons.ltv}
              colorClass="text-slate-700"
            />
          </section>

          {/* Charts Section */}
          <section className="grid grid-cols-1 lg:grid-cols-2 gap-6" aria-label="Analytics charts">
            {/* Pie Chart */}
            <div className="card p-5">
              <h2 className="text-base font-semibold text-slate-900 mb-4 text-center">Decision Distribution</h2>
              <div className="h-[300px] w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={pieData}
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={100}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {pieData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={PIE_COLORS[index % PIE_COLORS.length]} stroke="rgba(255,255,255,0.5)" strokeWidth={2} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value) => [value, 'Applications']} />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Bar Chart */}
            <div className="card p-5">
              <h2 className="text-base font-semibold text-slate-900 mb-4 text-center">Loan Amount Distribution</h2>
              <div className="h-[300px] w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={data.loanAmountDistribution || []}
                    margin={{ top: 20, right: 30, left: 0, bottom: 25 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e2e8f0" />
                    <XAxis 
                      dataKey="rangeLabel" 
                      tick={{ fill: '#64748b', fontSize: 12 }} 
                      angle={-45} 
                      textAnchor="end"
                      height={50}
                      axisLine={false}
                      tickLine={false}
                    />
                    <YAxis 
                      allowDecimals={false} 
                      tick={{ fill: '#64748b', fontSize: 12 }} 
                      axisLine={false}
                      tickLine={false}
                    />
                    <Tooltip 
                      cursor={{ fill: '#f8fafc' }}
                      contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                    />
                    <Bar dataKey="count" fill="#4f46e5" radius={[4, 4, 0, 0]} name="Applications" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
          </section>

          {/* Recent Applications (Top 3) */}
          <section className="card mt-6" aria-label="Recent applications">
            <div className="px-5 py-4 border-b border-slate-200 flex items-center justify-between">
              <h2 className="text-base font-semibold text-slate-900">Recent Applications</h2>
              <Link
                to="/applications"
                className="text-sm text-brand-600 font-medium hover:text-brand-700"
              >
                View all →
              </Link>
            </div>
            <div className="p-4">
              <RecentApplications applications={data.recentApplications ? data.recentApplications.slice(0, 3) : []} />
            </div>
          </section>
        </>
      )}
    </main>
  );
}
