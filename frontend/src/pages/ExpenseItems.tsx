import { Loader2 } from 'lucide-react'
import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { getCatalog } from '../api/catalogs'
import { getErrorMessage } from '../api/axios'
import {
  createExpenseItem, getExpenseItems, setExpenseItemStatus, updateExpenseItem, type ExpenseItemForm,
} from '../api/items'
import Badge from '../components/Badge'
import { ErrorBox, Field, inputClass } from '../components/Field'
import FormActions from '../components/FormActions'
import Modal from '../components/Modal'
import OwnerSelect from '../components/OwnerSelect'
import PageHeader from '../components/PageHeader'
import RowActions from '../components/RowActions'
import type { CatalogItem, ExpenseItem } from '../types'

const empty: ExpenseItemForm = { expenseTypeId: '', categoryId: '', defaultPaymentTypeId: '', description: '' }

export default function ExpenseItems() {
  const [owner, setOwner] = useState('') // '' = my account (only an admin can change it)
  const [items, setItems] = useState<ExpenseItem[]>([])
  const [types, setTypes] = useState<CatalogItem[]>([])
  const [categories, setCategories] = useState<CatalogItem[]>([])
  const [payments, setPayments] = useState<CatalogItem[]>([])
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState('')

  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<ExpenseItem | null>(null)
  const [form, setForm] = useState<ExpenseItemForm>(empty)
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    try {
      const [i, t, c, p] = await Promise.all([
        getExpenseItems(owner || undefined), getCatalog('expense-types', owner || undefined),
        getCatalog('categories', owner || undefined), getCatalog('payment-types', owner || undefined),
      ])
      setItems(i); setTypes(t); setCategories(c); setPayments(p)
      setPageError('')
    } catch (err) {
      setPageError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [owner])

  useEffect(() => { load() }, [load])

  const openForm = (item: ExpenseItem | null) => {
    setEditing(item)
    setForm(item
      ? { expenseTypeId: item.expenseTypeId, categoryId: item.categoryId,
          defaultPaymentTypeId: item.defaultPaymentTypeId, description: item.description }
      : empty)
    setFormError('')
    setOpen(true)
  }

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setFormError('')
    setSaving(true)
    try {
      if (editing) await updateExpenseItem(editing.id, form, owner || undefined)
      else await createExpenseItem(form, owner || undefined)
      setOpen(false)
      await load()
    } catch (err) {
      setFormError(getErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (item: ExpenseItem) => {
    try {
      await setExpenseItemStatus(item.id, !item.isActive, owner || undefined)
      await load()
    } catch (err) {
      setPageError(getErrorMessage(err))
    }
  }

  // Active options only, plus the one the edited record already has.
  const options = (list: CatalogItem[], current: string) =>
    list.filter((c) => c.isActive || c.id === current).map((c) => (
      <option key={c.id} value={c.id}>{c.description}</option>
    ))

  const set = (key: keyof ExpenseItemForm, value: string) => setForm((f) => ({ ...f, [key]: value }))

  return (
    <div>
      <PageHeader title="Egresos" subtitle="Tus conceptos de gasto: los eliges al registrar una transacción."
        actionLabel="Nuevo egreso" onAction={() => openForm(null)} />
      <OwnerSelect value={owner} onChange={setOwner} />
      <ErrorBox message={pageError} />

      <div className="mt-4 overflow-x-auto rounded-xl border border-line bg-surface">
        <table className="w-full min-w-[720px] text-left text-sm">
          <thead className="border-b border-line text-muted">
            <tr>
              {['Descripción', 'Tipo', 'Renglón', 'Pago por defecto', 'Estado', ''].map((h) => (
                <th key={h} className="px-4 py-3 font-semibold">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={6} className="px-4 py-10 text-center text-muted"><Loader2 className="mx-auto animate-spin" /></td></tr>
            ) : items.length === 0 ? (
              <tr><td colSpan={6} className="px-4 py-10 text-center text-muted">
                Aún no hay egresos. Crea el primero con “Nuevo egreso”.
              </td></tr>
            ) : items.map((i) => (
              <tr key={i.id} className="border-b border-line last:border-0">
                <td className="px-4 py-3 font-semibold">{i.description}</td>
                <td className="px-4 py-3">{i.expenseTypeName}</td>
                <td className="px-4 py-3">{i.categoryName}</td>
                <td className="px-4 py-3">{i.defaultPaymentTypeName}</td>
                <td className="px-4 py-3"><Badge active={i.isActive} /></td>
                <td className="px-4 py-3">
                  <RowActions name={i.description} isActive={i.isActive} onEdit={() => openForm(i)} onToggle={() => toggle(i)} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {open && (
        <Modal title={editing ? 'Editar egreso' : 'Nuevo egreso'} onClose={() => setOpen(false)}>
          <form onSubmit={onSubmit} className="space-y-4">
            <Field label="Descripción">
              <input className={inputClass} value={form.description} onChange={(e) => set('description', e.target.value)}
                required minLength={2} maxLength={120} autoFocus />
            </Field>
            <Field label="Tipo de egreso">
              <select className={inputClass} value={form.expenseTypeId} onChange={(e) => set('expenseTypeId', e.target.value)} required>
                <option value="" disabled>Selecciona…</option>
                {options(types, form.expenseTypeId)}
              </select>
            </Field>
            <Field label="Renglón">
              <select className={inputClass} value={form.categoryId} onChange={(e) => set('categoryId', e.target.value)} required>
                <option value="" disabled>Selecciona…</option>
                {options(categories, form.categoryId)}
              </select>
            </Field>
            <Field label="Tipo de pago por defecto">
              <select className={inputClass} value={form.defaultPaymentTypeId}
                onChange={(e) => set('defaultPaymentTypeId', e.target.value)} required>
                <option value="" disabled>Selecciona…</option>
                {options(payments, form.defaultPaymentTypeId)}
              </select>
            </Field>
            <ErrorBox message={formError} />
            <FormActions saving={saving} label={editing ? 'Guardar cambios' : 'Crear egreso'} onCancel={() => setOpen(false)} />
          </form>
        </Modal>
      )}
    </div>
  )
}
