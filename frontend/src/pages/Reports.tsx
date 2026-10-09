import { FileDown, FileSpreadsheet, Loader2 } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { getErrorMessage } from '../api/axios'
import { getReport } from '../api/reports'
import { getUsers } from '../api/users'
import { ErrorBox, inputClass } from '../components/Field'
import PageHeader from '../components/PageHeader'
import { useAuth } from '../context/AuthContext'
import type { Report, ReportFilters, ReportKind, User } from '../types'
import { exportReportCsv, exportReportPdf } from '../utils/exportReport'
import { money } from '../utils/format'
import { buildReportTable, periodText } from '../utils/reportTable'

const TABS: { kind: ReportKind; label: string }[] = [
  { kind: 'range', label: 'Por fechas' },
  { kind: 'by-user', label: 'Por usuario' },
  { kind: 'by-type', label: 'Por tipo' },
  { kind: 'by-category', label: 'Por categoría' },
  { kind: 'monthly', label: 'Resumen mensual' },
]

export default function Reports() {
  const { isAdmin } = useAuth()
  const [kind, setKind] = useState<ReportKind>('range')
  const [filters, setFilters] = useState<ReportFilters>({ year: new Date().getFullYear() })
  const [users, setUsers] = useState<User[]>([])
  const [report, setReport] = useState<Report | null>(null)
  const [loading, setLoading] = useState(true)
  const [exporting, setExporting] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (isAdmin) getUsers().then(setUsers).catch((err) => setError(getErrorMessage(err)))
  }, [isAdmin])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    getReport(kind, filters)
      .then((r) => { if (!cancelled) { setReport(r); setError('') } })
      .catch((err) => { if (!cancelled) setError(getErrorMessage(err)) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [kind, filters])

  const update = <K extends keyof ReportFilters>(key: K, value: ReportFilters[K] | '') =>
    setFilters((f) => ({ ...f, [key]: value === '' ? undefined : value }))

  // En "por usuario" el filtro de usuario no tiene sentido; en "mensual" se usa año en vez de fechas.
  const showUserFilter = isAdmin && kind !== 'by-user'
  const showUserColumn = isAdmin && !filters.userId
  const table = useMemo(() => (report ? buildReportTable(report, money, showUserColumn) : null), [report, showUserColumn])

  const years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i)
  const label = 'mb-1 block text-xs font-semibold text-muted'

  const onPdf = async () => {
    if (!report) return
    setExporting(true)
    try { await exportReportPdf(report, showUserColumn) } finally { setExporting(false) }
  }

  const summary: [string, number, string][] = report ? [
    ['Ingresos', report.totals.income, 'text-brand-dark'],
    ['Egresos', report.totals.expense, 'text-danger'],
    ['Balance', report.totals.balance, report.totals.balance < 0 ? 'text-danger' : 'text-ink'],
  ] : []

  return (
    <div>
      <PageHeader title="Reportes"
        subtitle={isAdmin ? 'Reportes de todos los usuarios, con exportación a PDF y Excel.' : 'Tus reportes, con exportación a PDF y Excel.'} />

      <div className="mb-4 flex gap-1 overflow-x-auto border-b border-line" role="tablist">
        {TABS.map((t) => (
          <button key={t.kind} role="tab" aria-selected={t.kind === kind} onClick={() => setKind(t.kind)}
            className={`-mb-px whitespace-nowrap border-b-2 px-4 py-2 text-sm font-semibold ${
              t.kind === kind ? 'border-brand text-brand-dark' : 'border-transparent text-muted hover:text-ink'}`}>
            {t.label}
          </button>
        ))}
      </div>

      <section className="mb-4 flex flex-wrap items-end gap-3 rounded-xl border border-line bg-white p-4" aria-label="Filtros">
        {showUserFilter && (
          <div className="min-w-48">
            <label className={label} htmlFor="r-user">Usuario</label>
            <select id="r-user" className={inputClass} value={filters.userId ?? ''} onChange={(e) => update('userId', e.target.value)}>
              <option value="">Todos</option>
              {users.map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
            </select>
          </div>
        )}
        {kind === 'monthly' ? (
          <div className="w-36">
            <label className={label} htmlFor="r-year">Año</label>
            <select id="r-year" className={inputClass} value={filters.year ?? ''} onChange={(e) => update('year', Number(e.target.value))}>
              {years.map((y) => <option key={y} value={y}>{y}</option>)}
            </select>
          </div>
        ) : (
          <>
            <div>
              <label className={label} htmlFor="r-from">Desde</label>
              <input id="r-from" type="date" className={inputClass} value={filters.from ?? ''} onChange={(e) => update('from', e.target.value)} />
            </div>
            <div>
              <label className={label} htmlFor="r-to">Hasta</label>
              <input id="r-to" type="date" className={inputClass} value={filters.to ?? ''} onChange={(e) => update('to', e.target.value)} />
            </div>
          </>
        )}

        <div className="ml-auto flex gap-2">
          <button onClick={onPdf} disabled={!report || loading || exporting}
            className="flex items-center gap-2 rounded-lg bg-brand px-4 py-2 text-sm font-bold text-white hover:bg-brand-dark disabled:opacity-50">
            {exporting ? <Loader2 size={16} className="animate-spin" /> : <FileDown size={16} />} PDF
          </button>
          <button onClick={() => report && exportReportCsv(report, showUserColumn)} disabled={!report || loading}
            className="flex items-center gap-2 rounded-lg border border-line bg-white px-4 py-2 text-sm font-bold hover:bg-paper disabled:opacity-50">
            <FileSpreadsheet size={16} /> Excel
          </button>
        </div>
      </section>

      <ErrorBox message={error} />

      {report && (
        <>
          <p className="mb-3 text-sm text-muted">{report.scope} · {periodText(report)} · {report.totals.count} transacciones activas</p>
          <div className="mb-4 grid gap-3 sm:grid-cols-3">
            {summary.map(([name, value, tone]) => (
              <div key={name} className="rounded-xl border border-line bg-white p-4">
                <p className="text-xs font-semibold text-muted">{name}</p>
                <p className={`mt-1 text-xl font-extrabold ${tone}`}>{money(value)}</p>
              </div>
            ))}
          </div>
        </>
      )}

      <div className="overflow-x-auto rounded-xl border border-line bg-white">
        <table className="w-full min-w-[560px] text-left text-sm">
          {table && (
            <thead className="border-b border-line text-muted">
              <tr>
                {table.columns.map((c) => (
                  <th key={c.label} className={`px-4 py-3 font-semibold ${c.align === 'right' ? 'text-right' : ''}`}>{c.label}</th>
                ))}
              </tr>
            </thead>
          )}
          <tbody>
            {loading ? (
              <tr><td className="px-4 py-10 text-center text-muted"><Loader2 className="mx-auto animate-spin" /></td></tr>
            ) : !table || table.rows.length === 0 ? (
              <tr><td className="px-4 py-10 text-center text-muted">No hay datos para estos filtros.</td></tr>
            ) : table.rows.map((row, i) => (
              <tr key={i} className="border-b border-line last:border-0">
                {row.map((cell, j) => (
                  <td key={j} className={`px-4 py-3 ${table.columns[j].align === 'right' ? 'whitespace-nowrap text-right' : ''}`}>{cell}</td>
                ))}
              </tr>
            ))}
          </tbody>
          {!loading && table?.footer && table.rows.length > 0 && (
            <tfoot className="border-t-2 border-line bg-paper font-bold">
              <tr>
                {table.footer.map((cell, j) => (
                  <td key={j} className={`px-4 py-3 ${table.columns[j].align === 'right' ? 'whitespace-nowrap text-right' : ''}`}>{cell}</td>
                ))}
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </div>
  )
}
