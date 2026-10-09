import { AlertTriangle, ArrowDownRight, ArrowUpRight, Gauge, Loader2, Wallet } from 'lucide-react'
import { useEffect, useState, type ReactNode } from 'react'
import {
  Bar, BarChart, CartesianGrid, Cell, Legend, Line, LineChart, Pie, PieChart,
  ResponsiveContainer, Tooltip, XAxis, YAxis,
} from 'recharts'
import { getErrorMessage } from '../api/axios'
import { getDashboard } from '../api/dashboard'
import { getUsers } from '../api/users'
import { ErrorBox, inputClass } from '../components/Field'
import TransactionTable from '../components/TransactionTable'
import { useAuth } from '../context/AuthContext'
import type { Dashboard as DashboardData, NameAmount, User } from '../types'
import { compactMoney, formatDate, money, monthShort } from '../utils/format'

const PALETTE = ['#0e6b5c', '#d6a24a', '#2f4858', '#8fbfb4', '#b4443a', '#9a6b8e', '#6b7773', '#3f8f80']
const GRID = '#e3e7e3'
const TICK = { fill: '#6b7773', fontSize: 12 }

function Card({ title, children, className = '' }: { title: string; children: ReactNode; className?: string }) {
  return (
    <section className={`rounded-xl border border-line bg-white p-5 ${className}`}>
      <h2 className="mb-4 text-sm font-bold">{title}</h2>
      {children}
    </section>
  )
}

const Empty = ({ text }: { text: string }) => (
  <p className="grid h-48 place-items-center text-center text-sm text-muted">{text}</p>
)

function Stat({ icon, label, value, tone }: { icon: ReactNode; label: string; value: string; tone: string }) {
  return (
    <div className="rounded-xl border border-line bg-white p-5">
      <p className="flex items-center gap-2 text-sm font-semibold text-muted">{icon}{label}</p>
      <p className={`mt-2 text-2xl font-extrabold tracking-tight ${tone}`}>{value}</p>
    </div>
  )
}

function Breakdown({ data }: { data: NameAmount[] }) {
  return (
    <ul className="mt-2 space-y-1.5 text-sm">
      {data.map((d, i) => (
        <li key={d.name} className="flex items-center justify-between gap-3">
          <span className="flex min-w-0 items-center gap-2">
            <span className="size-2.5 shrink-0 rounded-full" style={{ background: PALETTE[i % PALETTE.length] }} />
            <span className="truncate">{d.name}</span>
          </span>
          <span className="shrink-0 text-muted">{money(d.amount)} · {d.percent}%</span>
        </li>
      ))}
    </ul>
  )
}

export default function Dashboard() {
  const { user, isAdmin } = useAuth()
  const [target, setTarget] = useState('') // '' = me · 'all' · user id
  const [users, setUsers] = useState<User[]>([])
  const [data, setData] = useState<DashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (isAdmin) getUsers().then(setUsers).catch(() => undefined)
  }, [isAdmin])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    getDashboard(target || undefined)
      .then((d) => { if (!cancelled) { setData(d); setError('') } })
      .catch((err) => { if (!cancelled) setError(getErrorMessage(err)) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [target])

  const evolution = (data?.evolution ?? []).map((e) => ({
    name: monthShort(e.year, e.month), Ingresos: e.income, Egresos: e.expense,
  }))
  const hasEvolution = evolution.some((e) => e.Ingresos > 0 || e.Egresos > 0)

  const pct = data?.limitUsedPercent ?? 0
  const limitTone = pct > 100 ? 'text-danger' : pct >= 80 ? 'text-warn' : 'text-ink'
  const barTone = pct > 100 ? 'bg-danger' : pct >= 80 ? 'bg-[#d6a24a]' : 'bg-brand'

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-2xl font-extrabold tracking-tight">Hola, {user?.name.split(' ')[0]}</h1>
          <p className="mt-1 text-sm text-muted">
            {data
              ? `${data.scope} · período del ${formatDate(data.periodStart)} al ${formatDate(data.periodEnd)}`
              : 'Cargando tu resumen…'}
          </p>
        </div>
        {isAdmin && (
          <div className="min-w-52">
            <label htmlFor="d-user" className="mb-1 block text-xs font-semibold text-muted">Ver resumen de</label>
            <select id="d-user" className={inputClass} value={target} onChange={(e) => setTarget(e.target.value)}>
              <option value="">Mis finanzas</option>
              <option value="all">Todos los usuarios</option>
              {users.filter((u) => u.id !== user?.id).map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
            </select>
          </div>
        )}
      </div>

      <ErrorBox message={error} />

      {loading && !data ? (
        <div className="grid h-64 place-items-center text-muted"><Loader2 className="animate-spin" /></div>
      ) : data && (
        <>
          {data.hasLimit && pct > 100 && (
            <div role="alert" className="flex items-center gap-3 rounded-xl bg-warn-soft p-4 text-sm font-medium text-warn">
              <AlertTriangle size={20} className="shrink-0" />
              Superaste tu límite de egresos del período: llevas {money(data.periodExpense)} de {money(data.expenseLimit)}.
            </div>
          )}

          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <div className="rounded-xl bg-brand-dark p-5 text-white">
              <p className="flex items-center gap-2 text-sm font-semibold text-white/70"><Wallet size={16} />Balance actual</p>
              <p className="mt-2 text-2xl font-extrabold tracking-tight">{money(data.currentBalance)}</p>
            </div>
            <Stat icon={<ArrowUpRight size={16} className="text-brand" />} label="Ingresos del período"
              value={money(data.periodIncome)} tone="text-brand-dark" />
            <Stat icon={<ArrowDownRight size={16} className="text-danger" />} label="Egresos del período"
              value={money(data.periodExpense)} tone="text-danger" />
            <div className="rounded-xl border border-line bg-white p-5">
              <p className="flex items-center gap-2 text-sm font-semibold text-muted"><Gauge size={16} />Límite utilizado</p>
              {data.hasLimit ? (
                <>
                  <p className={`mt-2 text-2xl font-extrabold tracking-tight ${limitTone}`}>{pct}%</p>
                  <div className="mt-2 h-2 overflow-hidden rounded-full bg-paper" role="progressbar"
                    aria-valuenow={Math.min(pct, 100)} aria-valuemin={0} aria-valuemax={100} aria-label="Límite utilizado">
                    <div className={`h-full rounded-full ${barTone}`} style={{ width: `${Math.min(pct, 100)}%` }} />
                  </div>
                  <p className="mt-1.5 text-xs text-muted">{money(data.periodExpense)} de {money(data.expenseLimit)}</p>
                </>
              ) : (
                <p className="mt-2 text-sm text-muted">
                  {data.isAllUsers ? 'Elige un usuario para ver su límite.' : 'Sin límite definido.'}
                </p>
              )}
            </div>
          </div>

          <Card title="Evolución de ingresos y egresos (últimos 6 períodos)">
            {hasEvolution ? (
              <div className="h-64" role="img" aria-label="Gráfico de líneas de ingresos y egresos por período">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={evolution} margin={{ top: 5, right: 10, left: 0, bottom: 0 }}>
                    <CartesianGrid stroke={GRID} vertical={false} />
                    <XAxis dataKey="name" tick={TICK} axisLine={false} tickLine={false} />
                    <YAxis tick={TICK} axisLine={false} tickLine={false} tickFormatter={(v) => compactMoney(Number(v))} width={48} />
                    <Tooltip formatter={(v) => money(Number(v))} />
                    <Legend />
                    <Line type="monotone" dataKey="Ingresos" stroke="#0e6b5c" strokeWidth={2.5} dot={{ r: 3 }} />
                    <Line type="monotone" dataKey="Egresos" stroke="#b4443a" strokeWidth={2.5} dot={{ r: 3 }} />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            ) : <Empty text="Aún no hay movimientos en los últimos períodos." />}
          </Card>

          <div className="grid gap-4 lg:grid-cols-2">
            <Card title="Gastos por categoría">
              {data.expensesByCategory.length ? (
                <>
                  <div className="h-52" role="img" aria-label="Gráfico circular de gastos por categoría">
                    <ResponsiveContainer width="100%" height="100%">
                      <PieChart>
                        <Pie data={data.expensesByCategory} dataKey="amount" nameKey="name" innerRadius={55} outerRadius={85} paddingAngle={2}>
                          {data.expensesByCategory.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
                        </Pie>
                        <Tooltip formatter={(v) => money(Number(v))} />
                      </PieChart>
                    </ResponsiveContainer>
                  </div>
                  <Breakdown data={data.expensesByCategory} />
                </>
              ) : <Empty text="No hay egresos en este período." />}
            </Card>

            <Card title="Ingresos por tipo">
              {data.incomesByType.length ? (
                <>
                  <div className="h-52" role="img" aria-label="Gráfico de barras de ingresos por tipo">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={data.incomesByType} margin={{ top: 5, right: 10, left: 0, bottom: 0 }}>
                        <CartesianGrid stroke={GRID} vertical={false} />
                        <XAxis dataKey="name" tick={TICK} axisLine={false} tickLine={false} />
                        <YAxis tick={TICK} axisLine={false} tickLine={false} tickFormatter={(v) => compactMoney(Number(v))} width={48} />
                        <Tooltip formatter={(v) => money(Number(v))} cursor={{ fill: '#f5f6f3' }} />
                        <Bar dataKey="amount" name="Monto" radius={[6, 6, 0, 0]}>
                          {data.incomesByType.map((_, i) => <Cell key={i} fill={PALETTE[i % PALETTE.length]} />)}
                        </Bar>
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                  <Breakdown data={data.incomesByType} />
                </>
              ) : <Empty text="No hay ingresos en este período." />}
            </Card>
          </div>

          <section>
            <h2 className="mb-3 text-sm font-bold">Últimas transacciones</h2>
            <TransactionTable rows={data.lastTransactions} loading={false} showUser={isAdmin && data.isAllUsers}
              emptyText="Todavía no hay transacciones. Registra la primera en Transacciones." />
          </section>
        </>
      )}
    </div>
  )
}
