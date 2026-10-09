import { AlertTriangle, X } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { getCatalog } from '../api/catalogs'
import { getErrorMessage } from '../api/axios'
import { getExpenseItems, getIncomeItems } from '../api/items'
import {
  createTransaction, getTransactions, setTransactionStatus, updateTransaction, type TransactionForm,
} from '../api/transactions'
import { getUsers } from '../api/users'
import { ErrorBox, Field, inputClass } from '../components/Field'
import FormActions from '../components/FormActions'
import Modal from '../components/Modal'
import PageHeader from '../components/PageHeader'
import Pagination from '../components/Pagination'
import RowActions from '../components/RowActions'
import TransactionTable from '../components/TransactionTable'
import { useAuth } from '../context/AuthContext'
import type { CatalogItem, ExpenseItem, IncomeItem, LimitWarning, Transaction, TransactionPage, User } from '../types'
import { money, todayISO } from '../utils/format'

const PAGE_SIZE = 15

const blank = (userId: string): TransactionForm => ({
  type: 'Expense', userId, itemId: '', paymentTypeId: '',
  transactionDate: todayISO(), amount: 0, cardNumber: '', comment: '',
})

export default function Transactions() {
  const { user, isAdmin } = useAuth()
  const [data, setData] = useState<TransactionPage | null>(null)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState('')
  const [warning, setWarning] = useState<LimitWarning | null>(null)

  const [expenseItems, setExpenseItems] = useState<ExpenseItem[]>([])
  const [incomeItems, setIncomeItems] = useState<IncomeItem[]>([])
  const [payments, setPayments] = useState<CatalogItem[]>([])
  const [users, setUsers] = useState<User[]>([])

  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<Transaction | null>(null)
  const [form, setForm] = useState<TransactionForm>(blank(user?.id ?? ''))
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      setData(await getTransactions({ page, pageSize: PAGE_SIZE }))
      setPageError('')
    } catch (err) {
      setPageError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [page])

  useEffect(() => { load() }, [load])

  useEffect(() => {
    if (isAdmin) getUsers().then(setUsers).catch((err) => setPageError(getErrorMessage(err)))
  }, [isAdmin])

  // Concepts and payment types are personal: load the ones of the user being recorded.
  // (A regular user always gets their own; the server ignores the parameter.)
  const ownerId = isAdmin ? form.userId : undefined
  useEffect(() => {
    Promise.all([getExpenseItems(ownerId), getIncomeItems(ownerId), getCatalog('payment-types', ownerId)])
      .then(([e, i, p]) => { setExpenseItems(e); setIncomeItems(i); setPayments(p) })
      .catch((err) => setPageError(getErrorMessage(err)))
  }, [ownerId])

  const selectedPayment = useMemo(() => payments.find((p) => p.id === form.paymentTypeId), [payments, form.paymentTypeId])

  const openForm = (t: Transaction | null) => {
    setEditing(t)
    setForm(t
      ? { type: t.type, userId: t.userId, itemId: t.itemId, paymentTypeId: t.paymentTypeId,
          transactionDate: t.transactionDate.slice(0, 10), amount: t.amount, cardNumber: '', comment: t.comment ?? '' }
      : blank(user?.id ?? ''))
    setFormError('')
    setOpen(true)
  }

  const set = <K extends keyof TransactionForm>(key: K, value: TransactionForm[K]) =>
    setForm((f) => ({ ...f, [key]: value }))

  const changeType = (type: TransactionForm['type']) =>
    setForm((f) => ({ ...f, type, itemId: '', paymentTypeId: '', cardNumber: '' }))

  // When an expense is chosen, its default payment type is suggested.
  const changeItem = (itemId: string) => {
    const exp = form.type === 'Expense' ? expenseItems.find((i) => i.id === itemId) : undefined
    setForm((f) => ({ ...f, itemId, paymentTypeId: exp?.defaultPaymentTypeId ?? f.paymentTypeId, cardNumber: '' }))
  }

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setFormError('')
    setSaving(true)
    try {
      const { type: _t, userId: _u, ...rest } = form
      void _t; void _u
      const res = editing
        ? await updateTransaction(editing.id, rest)
        : await createTransaction({ ...form, userId: isAdmin ? form.userId : undefined })
      setWarning(res.warning ?? null)
      setOpen(false)
      if (page !== 1 && !editing) setPage(1)
      else await load()
    } catch (err) {
      setFormError(getErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (t: Transaction) => {
    try {
      await setTransactionStatus(t.id, !t.isActive)
      await load()
    } catch (err) {
      setPageError(getErrorMessage(err))
    }
  }

  const items = form.type === 'Expense' ? expenseItems : incomeItems
  const itemOptions = items
    .filter((i) => i.isActive || i.id === form.itemId)
    .map((i) => <option key={i.id} value={i.id}>{i.description}</option>)

  return (
    <div>
      <PageHeader title="Transacciones"
        subtitle={isAdmin ? 'Registra ingresos y egresos de cualquier usuario.' : 'Registra tus ingresos y egresos.'}
        actionLabel="Nueva transacción" onAction={() => openForm(null)} />

      {warning && (
        <div role="alert" className="mb-4 flex items-start gap-3 rounded-xl bg-warn-soft p-4 text-warn">
          <AlertTriangle size={20} className="mt-0.5 shrink-0" />
          <div className="flex-1 text-sm">
            <p className="font-bold">Límite de egresos superado ({warning.percent}%)</p>
            <p>{warning.message}</p>
          </div>
          <button onClick={() => setWarning(null)} aria-label="Cerrar aviso" className="rounded p-1 hover:bg-white/50">
            <X size={16} />
          </button>
        </div>
      )}

      <ErrorBox message={pageError} />

      <div className="mt-4">
        <TransactionTable rows={data?.items ?? []} loading={loading} showUser={isAdmin}
          emptyText="Aún no hay transacciones. Registra la primera con “Nueva transacción”."
          actions={(t) => (
            <RowActions name={`transacción ${t.itemDescription}`} isActive={t.isActive}
              onEdit={() => openForm(t)} onToggle={() => toggle(t)} />
          )} />
        {data && <Pagination page={data.page} pageSize={data.pageSize} total={data.total} onChange={setPage} />}
      </div>

      {open && (
        <Modal title={editing ? 'Editar transacción' : 'Nueva transacción'} onClose={() => setOpen(false)}>
          <form onSubmit={onSubmit} className="space-y-4">
            {!editing && (
              <div className="grid grid-cols-2 gap-1 rounded-lg bg-paper p-1" role="radiogroup" aria-label="Tipo de transacción">
                {(['Expense', 'Income'] as const).map((t) => (
                  <button type="button" key={t} role="radio" aria-checked={form.type === t} onClick={() => changeType(t)}
                    className={`rounded-md py-2 text-sm font-bold ${
                      form.type === t ? 'bg-white shadow-sm ' + (t === 'Income' ? 'text-brand-dark' : 'text-danger') : 'text-muted'}`}>
                    {t === 'Expense' ? 'Egreso' : 'Ingreso'}
                  </button>
                ))}
              </div>
            )}

            {isAdmin && !editing && (
              <Field label="Usuario">
                <select className={inputClass} value={form.userId}
                  onChange={(e) => setForm((f) => ({ ...f, userId: e.target.value, itemId: '', paymentTypeId: '', cardNumber: '' }))} required>
                  {users.filter((u) => u.isActive).map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
                </select>
              </Field>
            )}

            <Field label={form.type === 'Expense' ? 'Egreso' : 'Ingreso'}>
              <select className={inputClass} value={form.itemId} onChange={(e) => changeItem(e.target.value)} required>
                <option value="" disabled>Selecciona…</option>
                {itemOptions}
              </select>
            </Field>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Monto">
                <input type="number" min={0.01} step="0.01" className={inputClass} required
                  value={form.amount || ''} onChange={(e) => set('amount', Number(e.target.value))} />
              </Field>
              <Field label="Fecha">
                <input type="date" max={todayISO()} className={inputClass} required
                  value={form.transactionDate} onChange={(e) => set('transactionDate', e.target.value)} />
              </Field>
            </div>

            <Field label="Tipo de pago">
              <select className={inputClass} value={form.paymentTypeId} onChange={(e) => set('paymentTypeId', e.target.value)} required>
                <option value="" disabled>Selecciona…</option>
                {payments.filter((p) => p.isActive || p.id === form.paymentTypeId)
                  .map((p) => <option key={p.id} value={p.id}>{p.description}</option>)}
              </select>
            </Field>

            {selectedPayment?.requiresCard && (
              <Field label={editing?.cardLast4 ? `Número de tarjeta (actual: ····${editing.cardLast4}, déjalo vacío para conservarla)` : 'Número de tarjeta'}>
                <input className={inputClass} inputMode="numeric" autoComplete="off" maxLength={23}
                  placeholder="Solo se guardan los últimos 4 dígitos"
                  value={form.cardNumber} onChange={(e) => set('cardNumber', e.target.value)}
                  required={!editing?.cardLast4} />
              </Field>
            )}

            <Field label="Comentario (opcional)">
              <textarea className={inputClass} rows={2} maxLength={500}
                value={form.comment} onChange={(e) => set('comment', e.target.value)} />
            </Field>

            {form.amount > 0 && (
              <p className="text-sm text-muted">
                {form.type === 'Expense' ? 'Egreso' : 'Ingreso'} de <strong className="text-ink">{money(form.amount)}</strong>
              </p>
            )}

            <ErrorBox message={formError} />
            <FormActions saving={saving} label={editing ? 'Guardar cambios' : 'Registrar'} onCancel={() => setOpen(false)} />
          </form>
        </Modal>
      )}
    </div>
  )
}
