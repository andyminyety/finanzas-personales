import { ArrowLeftRight, CalendarCheck, FileBarChart, LayoutDashboard, ListChecks, LogOut, Search, TrendingDown, TrendingUp, UserCircle, Users, Wallet, type LucideIcon } from 'lucide-react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

interface NavItem {
  to: string
  label: string
  icon: LucideIcon
  adminOnly?: boolean
}

// Menu entries. Those marked adminOnly are only visible to administrators.
const NAV: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/transacciones', label: 'Transacciones', icon: ArrowLeftRight },
  { to: '/consultas', label: 'Consultas', icon: Search },
  { to: '/cortes', label: 'Cortes', icon: CalendarCheck },
  { to: '/reportes', label: 'Reportes', icon: FileBarChart },
  { to: '/egresos', label: 'Egresos', icon: TrendingDown },
  { to: '/ingresos', label: 'Ingresos', icon: TrendingUp },
  { to: '/catalogos', label: 'Catálogos', icon: ListChecks },
  { to: '/usuarios', label: 'Usuarios', icon: Users, adminOnly: true },
  { to: '/perfil', label: 'Mi perfil', icon: UserCircle },
]

export default function Layout() {
  const { user, isAdmin, logout } = useAuth()
  const navigate = useNavigate()
  const items = NAV.filter((i) => !i.adminOnly || isAdmin)

  const onLogout = () => {
    logout()
    navigate('/login')
  }

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-semibold transition-colors ${
      isActive ? 'bg-brand-soft text-brand-dark' : 'text-muted hover:bg-paper hover:text-ink'
    }`

  return (
    <div className="min-h-screen md:flex">
      <aside className="hidden w-64 shrink-0 flex-col border-r border-line bg-white p-4 md:flex">
        <div className="mb-8 flex items-center gap-2.5 px-2 pt-2">
          <span className="grid size-9 place-items-center rounded-lg bg-brand text-white">
            <Wallet size={18} />
          </span>
          <span className="text-base font-extrabold tracking-tight">Finanzas</span>
        </div>

        <nav className="flex flex-1 flex-col gap-1">
          {items.map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} end={to === '/'} className={linkClass}>
              <Icon size={18} />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="border-t border-line pt-4">
          <p className="truncate px-2 text-sm font-semibold">{user?.name}</p>
          <p className="mb-3 truncate px-2 text-xs text-muted">{isAdmin ? 'Administrador' : 'Usuario'}</p>
          <button onClick={onLogout}
            className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-semibold text-muted hover:bg-danger-soft hover:text-danger">
            <LogOut size={18} />
            Cerrar sesión
          </button>
        </div>
      </aside>

      {/* Top bar on mobile */}
      <header className="flex items-center justify-between border-b border-line bg-white px-4 py-3 md:hidden">
        <span className="flex items-center gap-2 font-extrabold">
          <Wallet size={18} className="text-brand" /> <span className="hidden sm:inline">Finanzas</span>
        </span>
        <nav className="flex items-center overflow-x-auto">
          {items.map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} end={to === '/'} aria-label={label}
              className={({ isActive }) =>
                `rounded-lg p-2 ${isActive ? 'bg-brand-soft text-brand-dark' : 'text-muted'}`}>
              <Icon size={20} />
            </NavLink>
          ))}
          <button onClick={onLogout} aria-label="Cerrar sesión" className="rounded-lg p-2 text-muted">
            <LogOut size={20} />
          </button>
        </nav>
      </header>

      <main className="min-w-0 flex-1 p-4 md:p-8">
        <Outlet />
      </main>
    </div>
  )
}
