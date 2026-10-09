import { Loader2 } from 'lucide-react'
import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { getCatalog } from '../api/catalogs'
import { getErrorMessage } from '../api/axios'
import {
  createIncomeItem, getIncomeItems, setIncomeItemStatus, updateIncomeItem, type IncomeItemForm,
} from '../api/items'
import Badge from '../components/Badge'
import { ErrorBox, Field, inputClass } from '../components/Field'
import FormActions from '../components/FormActions'
import Modal from '../components/Modal'
import OwnerSelect from '../components/OwnerSelect'
import PageHeader from '../components/PageHeader'
import RowActions from '../components/RowActions'
import type { CatalogItem, IncomeItem } from '../types'

const empty: IncomeItemForm = { incomeTypeId: '', description: '', source: '' }

export default function IncomeItems() {
  const [owner, setOwner] = useState('') // '' = my account (only an admin can change it)
  const [items, setItems] = useState<IncomeItem[]>([])
  const [types, setTypes] = useState<CatalogItem[]>([])
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState('')

  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<IncomeItem | null>(null)
  const [form, setForm] = useState<IncomeItemForm>(empty)
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    try {
      const [i, t] = await Promise.all([getIncomeItems(owner || undefined), getCatalog('income-types', owner || undefined)])
      setItems(i); setTypes(t)
      setPageError('')
    } catch (err) {
      setPageError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [owner])

  useEffect(() => { load() }, [load])

  const openForm = (item: IncomeItem | null) => {
    setEditing(item)
    setForm(item ? { incomeTypeId: item.incomeTypeId, description: item.description, source: item.source } : empty)
    setFormError('')
    setOpen(true)
  }

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setFormError('')
    setSaving(true)
    try {
      if (editing) await updateIncomeItem(editing.id, form, owner || undefined)
      else await createIncomeItem(form, owner || undefined)
      setOpen(false)
      await load()
    } catch (err) {
      setFormError(getErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (item: IncomeItem) => {
    try {
      await setIncomeItemStatus(item.id, !item.isActive, owner || undefined)
      await load()
    } catch (err) {
      setPageError(getErrorMessage(err))
    }
  }

  const set = (key: keyof IncomeItemForm, value: string) => setForm((f) => ({ ...f, [key]: value }))

  return (
    <div>
      <PageHeader title="Ingresos" subtitle="Tus conceptos de ingreso: los eliges al registrar una transacción."
        actionLabel="Nuevo ingreso" onAction={() => openForm(null)} />
      <OwnerSelect value={owner} onChange={setOwner} />
      <ErrorBox message={pageError} />

      <div className="mt-4 overflow-x-auto rounded-xl border border-line bg-white">
        <table className="w-full min-w-[640px] text-left text-sm">
          <thead className="border-b border-line text-muted">
            <tr>
              {['Descripción', 'Tipo', 'Institución / Empleador / Cliente', 'Estado', ''].map((h) => (
                <th key={h} className="px-4 py-3 font-semibold">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={5} className="px-4 py-10 text-center text-muted"><Loader2 className="mx-auto animate-spin" /></td></tr>
            ) : items.length === 0 ? (
              <tr><td colSpan={5} className="px-4 py-10 text-center text-muted">
                Aún no hay ingresos. Crea el primero con “Nuevo ingreso”.
              </td></tr>
            ) : items.map((i) => (
              <tr key={i.id} className="border-b border-line last:border-0">
                <td className="px-4 py-3 font-semibold">{i.description}</td>
                <td className="px-4 py-3">{i.incomeTypeName}</td>
                <td className="px-4 py-3">{i.source}</td>
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
        <Modal title={editing ? 'Editar ingreso' : 'Nuevo ingreso'} onClose={() => setOpen(false)}>
          <form onSubmit={onSubmit} className="space-y-4">
            <Field label="Descripción">
              <input className={inputClass} value={form.description} onChange={(e) => set('description', e.target.value)}
                required minLength={2} maxLength={120} autoFocus />
            </Field>
            <Field label="Tipo de ingreso">
              <select className={inputClass} value={form.incomeTypeId} onChange={(e) => set('incomeTypeId', e.target.value)} required>
                <option value="" disabled>Selecciona…</option>
                {types.filter((t) => t.isActive || t.id === form.incomeTypeId).map((t) => (
                  <option key={t.id} value={t.id}>{t.description}</option>
                ))}
              </select>
            </Field>
            <Field label="Institución / Empleador / Cliente">
              <input className={inputClass} value={form.source} onChange={(e) => set('source', e.target.value)}
                required minLength={2} maxLength={120} />
            </Field>
            <ErrorBox message={formError} />
            <FormActions saving={saving} label={editing ? 'Guardar cambios' : 'Crear ingreso'} onCancel={() => setOpen(false)} />
          </form>
        </Modal>
      )}
    </div>
  )
}
