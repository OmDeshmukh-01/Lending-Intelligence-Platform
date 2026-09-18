import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Sidebar from "./components/Sidebar";
import Dashboard from "./pages/Dashboard";
import NewApplication from "./pages/NewApplication";
import Applications from "./pages/Applications";
import ApplicationDetail from "./pages/ApplicationDetail";

export default function App() {
  return (
    <BrowserRouter>
      <div className="flex flex-col lg:flex-row min-h-screen bg-slate-50">
        <Sidebar />
        <div className="flex-1 flex flex-col min-w-0">
          <Routes>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/applications/new" element={<NewApplication />} />
            <Route path="/applications/:id" element={<ApplicationDetail />} />
            <Route path="/applications" element={<Applications />} />
            {/* Catch-all */}
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </div>
      </div>
    </BrowserRouter>
  );
}
