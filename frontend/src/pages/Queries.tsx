import { FilterX } from 'lucide-react'
import { useEffect, useState } from 'react'
import { getCatalog } from '../api/catalogs'
import { getErrorMessage } from '../api/axios'
import { getTransactions } from '../api/transactions'
import { getUsers } from '../api/users'
import { ErrorBox, inputClass } from '../components/Field'
import PageHeader from '../components/PageHeader'
import Pagination from '../components/Pagination'
import TransactionTable from '../components/TransactionTable'
import { useAuth } from '../context/AuthContext'
import type { CatalogItem, TransactionFilters, TransactionPage, User } from '../types'
import { money } from '../utils/format'

const PAGE_SIZE = 20

export default function Queries() {
  const { isAdmin } = useAuth()
  const [filters, setFilters] = useState<TransactionFilters>({ isActive: true })
  const [page, setPage] = useState(1)
  const [data, setData] = useState<TransactionPage | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [users, setUsers] = useState<User[]>([])
  const [expenseTypes, setExpenseTypes] = useState<CatalogItem[]>([])
  const [categories, setCategories] = useState<CatalogItem[]>([])
  const [payments, setPayments] = useState<CatalogItem[]>([])

  useEffect(() => {
    if (isAdmin) getUsers().then(setUsers).catch((err) => setError(getErrorMessage(err)))
  }, [isAdmin])

  // Catalogs are personal: the admin can only filter by them once a user is chosen.
  const catalogUser = isAdmin ? filters.userId : undefined
  useEffect(() => {
    if (isAdmin && !catalogUser) {
      setExpenseTypes([]); setCategories([]); setPayments([])
      return
    }
    Promise.all([getCatalog('expense-types', catalogUser), getCatalog('categories', catalogUser), getCatalog('payment-types', catalogUser)])
      .then(([t, c, p]) => { setExpenseTypes(t); setCategories(c); setPayments(p) })
      .catch((err) => setError(getErrorMessage(err)))
  }, [isAdmin, catalogUser])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    getTransactions({ ...filters, page, pageSize: PAGE_SIZE })
      .then((d) => { if (!cancelled) { setData(d); setError('') } })
      .catch((err) => { if (!cancelled) setError(getErrorMessage(err)) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [filters, page])

  const update = <K extends keyof TransactionFilters>(key: K, value: TransactionFilters[K]) => {
    setPage(1)
    setFilters((f) => ({ ...f, [key]: value === '' ? undefined : value }))
  }

  // Expense type and category only apply to expenses.
  const onlyIncome = filters.type === 'Income'
  const needsUser = isAdmin && !filters.userId
  const balance = (data?.totalIncome ?? 0) - (data?.totalExpense ?? 0)

  const label = 'mb-1 block text-xs font-semibold text-muted'
  const clear = () => { setPage(1); setFilters({ isActive: true }) }

  return (
    <div>
      <PageHeader title="Consultas"
        subtitle={isAdmin ? 'Busca transacciones de todos los usuarios.' : 'Busca entre tus transacciones.'} />

      <section className="mb-4 rounded-xl border border-line bg-surface p-4" aria-label="Filtros">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {isAdmin && (
            <div>
              <label className={label} htmlFor="f-user">Usuario</label>
              <select id="f-user" className={inputClass} value={filters.userId ?? ''} onChange={(e) => {
                setPage(1)
                setFilters((f) => ({ ...f, userId: e.target.value || undefined,
                  expenseTypeId: undefined, categoryId: undefined, paymentTypeId: undefined }))
              }}>
                <option value="">Todos</option>
                {users.map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
              </select>
            </div>
          )}
          <div>
            <label className={label} htmlFor="f-from">Desde</label>
            <input id="f-from" type="date" className={inputClass} value={filters.from ?? ''} onChange={(e) => update('from', e.target.value)} />
          </div>
          <div>
            <label className={label} htmlFor="f-to">Hasta</label>
            <input id="f-to" type="date" className={inputClass} value={filters.to ?? ''} onChange={(e) => update('to', e.target.value)} />
          </div>
          <div>
            <label className={label} htmlFor="f-type">Tipo de transacción</label>
            <select id="f-type" className={inputClass} value={filters.type ?? ''}
              onChange={(e) => {
                const v = e.target.value as TransactionFilters['type']
                setPage(1)
                setFilters((f) => ({ ...f, type: v || undefined,
                  ...(v === 'Income' ? { expenseTypeId: undefined, categoryId: undefined } : {}) }))
              }}>
              <option value="">Ingresos y egresos</option>
              <option value="Income">Ingresos</option>
              <option value="Expense">Egresos</option>
            </select>
          </div>
          <div>
            <label className={label} htmlFor="f-etype">Tipo de gasto</label>
            <select id="f-etype" className={inputClass} disabled={onlyIncome || needsUser} value={filters.expenseTypeId ?? ''}
              onChange={(e) => update('expenseTypeId', e.target.value)}>
              <option value="">Todos</option>
              {expenseTypes.map((c) => <option key={c.id} value={c.id}>{c.description}</option>)}
            </select>
          </div>
          <div>
            <label className={label} htmlFor="f-cat">Categoría</label>
            <select id="f-cat" className={inputClass} disabled={onlyIncome || needsUser} value={filters.categoryId ?? ''}
              onChange={(e) => update('categoryId', e.target.value)}>
              <option value="">Todas</option>
              {categories.map((c) => <option key={c.id} value={c.id}>{c.description}</option>)}
            </select>
          </div>
          <div>
            <label className={label} htmlFor="f-pay">Tipo de pago</label>
            <select id="f-pay" className={inputClass} disabled={needsUser} value={filters.paymentTypeId ?? ''}
              onChange={(e) => update('paymentTypeId', e.target.value)}>
              <option value="">Todos</option>
              {payments.map((c) => <option key={c.id} value={c.id}>{c.description}</option>)}
            </select>
          </div>
          <div>
            <label className={label} htmlFor="f-status">Estado</label>
            <select id="f-status" className={inputClass}
              value={filters.isActive === undefined ? 'all' : String(filters.isActive)}
              onChange={(e) => update('isActive', e.target.value === 'all' ? undefined : e.target.value === 'true')}>
              <option value="true">Activas</option>
              <option value="false">Inhabilitadas</option>
              <option value="all">Todas</option>
            </select>
          </div>
        </div>
        {needsUser && (
          <p className="mt-3 text-xs text-muted">
            Elige un usuario para filtrar por tipo de gasto, categoría o tipo de pago (cada usuario tiene los suyos).
          </p>
        )}
        <button onClick={clear} className="mt-3 flex items-center gap-2 text-sm font-semibold text-muted hover:text-ink">
          <FilterX size={16} /> Limpiar filtros
        </button>
      </section>

      <div className="mb-4 grid gap-3 sm:grid-cols-3">
        <div className="rounded-xl border border-line bg-surface p-4">
          <p className="text-xs font-semibold text-muted">Ingresos</p>
          <p className="mt-1 text-xl font-extrabold text-brand-text">{money(data?.totalIncome ?? 0)}</p>
        </div>
        <div className="rounded-xl border border-line bg-surface p-4">
          <p className="text-xs font-semibold text-muted">Egresos</p>
          <p className="mt-1 text-xl font-extrabold text-danger">{money(data?.totalExpense ?? 0)}</p>
        </div>
        <div className="rounded-xl border border-line bg-surface p-4">
          <p className="text-xs font-semibold text-muted">Balance</p>
          <p className={`mt-1 text-xl font-extrabold ${balance < 0 ? 'text-danger' : 'text-ink'}`}>{money(balance)}</p>
        </div>
      </div>
      <p className="-mt-2 mb-4 text-xs text-muted">Los totales cuentan solo las transacciones activas que cumplen los filtros.</p>

      <ErrorBox message={error} />

      <div className="mt-4">
        <TransactionTable rows={data?.items ?? []} loading={loading} showUser={isAdmin}
          emptyText="No hay transacciones con esos filtros." />
        {data && <Pagination page={data.page} pageSize={data.pageSize} total={data.total} onChange={setPage} />}
      </div>
    </div>
  )
}
