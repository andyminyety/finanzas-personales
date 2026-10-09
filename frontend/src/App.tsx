import { Loader2 } from 'lucide-react'
import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import ProtectedRoute from './components/ProtectedRoute'
import Login from './pages/Login'

// Each page is downloaded only when visited (the dashboard and reports load heavy libraries).
const Dashboard = lazy(() => import('./pages/Dashboard'))
const Transactions = lazy(() => import('./pages/Transactions'))
const Queries = lazy(() => import('./pages/Queries'))
const Cuts = lazy(() => import('./pages/Cuts'))
const Reports = lazy(() => import('./pages/Reports'))
const ExpenseItems = lazy(() => import('./pages/ExpenseItems'))
const IncomeItems = lazy(() => import('./pages/IncomeItems'))
const Catalogs = lazy(() => import('./pages/Catalogs'))
const Users = lazy(() => import('./pages/Users'))
const Profile = lazy(() => import('./pages/Profile'))

const Loading = () => (
  <div className="grid h-64 place-items-center text-muted" role="status" aria-label="Cargando">
    <Loader2 className="animate-spin" />
  </div>
)

export default function App() {
  return (
    <Suspense fallback={<Loading />}>
      <Routes>
        <Route path="/login" element={<Login />} />

        <Route element={<ProtectedRoute />}>
          <Route element={<Layout />}>
            <Route path="/" element={<Dashboard />} />
            <Route path="/transacciones" element={<Transactions />} />
            <Route path="/consultas" element={<Queries />} />
            <Route path="/cortes" element={<Cuts />} />
            <Route path="/reportes" element={<Reports />} />
            <Route path="/egresos" element={<ExpenseItems />} />
            <Route path="/ingresos" element={<IncomeItems />} />
            <Route path="/catalogos" element={<Catalogs />} />
            <Route path="/perfil" element={<Profile />} />

            <Route element={<ProtectedRoute adminOnly />}>
              <Route path="/usuarios" element={<Users />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  )
}
