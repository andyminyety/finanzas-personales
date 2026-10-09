import { CalendarCheck, CheckCircle2, Info, Loader2, Scissors } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { getErrorMessage } from '../api/axios'
import { getCuts, runCuts } from '../api/cuts'
import { getUsers } from '../api/users'
import { ErrorBox, inputClass } from '../components/Field'
import PageHeader from '../components/PageHeader'
import { useAuth } from '../context/AuthContext'
import type { MonthlyCut, User } from '../types'
import { formatDate, money, monthLabel } from '../utils/format'

export default function Cuts() {
  const { isAdmin } = useAuth()
  const [cuts, setCuts] = useState<MonthlyCut[]>([])
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [filterUser, setFilterUser] = useState('')
  const [year, setYear] = useState('')
  const [runUser, setRunUser] = useState('')
  const [running, setRunning] = useState(false)
  const [result, setResult] = useState<{ message: string; count: number } | null>(null)

  const thisYear = new Date().getFullYear()
  const years = Array.from({ length: 5 }, (_, i) => thisYear - i)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      setCuts(await getCuts({ userId: filterUser || undefined, year: year ? Number(year) : undefined }))
      setError('')
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [filterUser, year])

  useEffect(() => { load() }, [load])

  useEffect(() => {
    if (isAdmin) getUsers().then(setUsers).catch((err) => setError(getErrorMessage(err)))
  }, [isAdmin])

  const onRun = async () => {
    setRunning(true)
    setResult(null)
    try {
      const res = await runCuts(isAdmin ? runUser : undefined)
      setResult({ message: res.message, count: res.created.length })
      await load()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setRunning(false)
    }
  }

  const headers = ['Período', ...(isAdmin ? ['Usuario'] : []), 'Fecha de corte', 'Balance inicial', 'Ingresos', 'Egresos', 'Balance al corte', 'Origen']

  return (
    <div>
      <PageHeader title="Cortes mensuales"
        subtitle="Cierran cada período con sus totales. Se generan solos al terminar el período; aquí puedes generarlos ahora." />

      <section className="mb-4 flex flex-wrap items-end gap-3 rounded-xl border border-line bg-white p-4" aria-label="Generar corte">
        {isAdmin && (
          <div className="min-w-48">
            <label htmlFor="run-user" className="mb-1 block text-xs font-semibold text-muted">Generar corte para</label>
            <select id="run-user" className={inputClass} value={runUser} onChange={(e) => setRunUser(e.target.value)}>
              <option value="">Todos los usuarios activos</option>
              {users.filter((u) => u.isActive).map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
            </select>
          </div>
        )}
        <button onClick={onRun} disabled={running}
          className="flex items-center gap-2 rounded-lg bg-brand px-4 py-2 text-sm font-bold text-white hover:bg-brand-dark disabled:opacity-60">
          {running ? <Loader2 size={16} className="animate-spin" /> : <Scissors size={16} />}
          Generar corte
        </button>
        <p className="flex basis-full items-start gap-2 text-xs text-muted">
          <Info size={14} className="mt-0.5 shrink-0" />
          Solo se cortan períodos ya terminados. Un período cortado queda cerrado: no se pueden crear ni modificar
          transacciones con fecha dentro de él.
        </p>
      </section>

      {result && (
        <div role="status" className="mb-4 flex items-center gap-2 rounded-lg bg-brand-soft px-4 py-3 text-sm font-medium text-brand-dark">
          <CheckCircle2 size={18} /> {result.message}
        </div>
      )}

      <div className="mb-3 flex flex-wrap gap-3">
        {isAdmin && (
          <div className="min-w-48">
            <label htmlFor="f-user" className="mb-1 block text-xs font-semibold text-muted">Usuario</label>
            <select id="f-user" className={inputClass} value={filterUser} onChange={(e) => setFilterUser(e.target.value)}>
              <option value="">Todos</option>
              {users.map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
            </select>
          </div>
        )}
        <div className="w-36">
          <label htmlFor="f-year" className="mb-1 block text-xs font-semibold text-muted">Año</label>
          <select id="f-year" className={inputClass} value={year} onChange={(e) => setYear(e.target.value)}>
            <option value="">Todos</option>
            {years.map((y) => <option key={y} value={y}>{y}</option>)}
          </select>
        </div>
      </div>

      <ErrorBox message={error} />

      <div className="mt-3 overflow-x-auto rounded-xl border border-line bg-white">
        <table className="w-full min-w-[820px] text-left text-sm">
          <thead className="border-b border-line text-muted">
            <tr>{headers.map((h) => <th key={h} className="px-4 py-3 font-semibold">{h}</th>)}</tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={headers.length} className="px-4 py-10 text-center text-muted"><Loader2 className="mx-auto animate-spin" /></td></tr>
            ) : cuts.length === 0 ? (
              <tr><td colSpan={headers.length} className="px-4 py-10 text-center text-muted">
                <CalendarCheck className="mx-auto mb-2" size={22} />
                Aún no hay cortes. Cuando termine un período se generará solo, o usa “Generar corte”.
              </td></tr>
            ) : cuts.map((c) => (
              <tr key={c.id} className="border-b border-line last:border-0">
                <td className="px-4 py-3">
                  <p className="font-semibold">{monthLabel(c.year, c.month)}</p>
                  <p className="text-xs text-muted">{formatDate(c.periodStart)} – {formatDate(c.periodEnd)}</p>
                </td>
                {isAdmin && <td className="px-4 py-3">{c.userName}</td>}
                <td className="whitespace-nowrap px-4 py-3">{formatDate(c.cutDate)}</td>
                <td className="whitespace-nowrap px-4 py-3">{money(c.initialBalance)}</td>
                <td className="whitespace-nowrap px-4 py-3 text-brand-dark">{money(c.totalIncome)}</td>
                <td className="whitespace-nowrap px-4 py-3 text-danger">{money(c.totalExpense)}</td>
                <td className={`whitespace-nowrap px-4 py-3 font-bold ${c.finalBalance < 0 ? 'text-danger' : ''}`}>
                  {money(c.finalBalance)}
                </td>
                <td className="px-4 py-3 text-muted">{c.source === 'Auto' ? 'Automático' : 'Manual'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
