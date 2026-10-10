import { Loader2 } from 'lucide-react'
import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { createCatalogItem, getCatalog, setCatalogStatus, updateCatalogItem } from '../api/catalogs'
import { getErrorMessage } from '../api/axios'
import Badge from '../components/Badge'
import { ErrorBox, Field, inputClass } from '../components/Field'
import FormActions from '../components/FormActions'
import Modal from '../components/Modal'
import OwnerSelect from '../components/OwnerSelect'
import PageHeader from '../components/PageHeader'
import RowActions from '../components/RowActions'
import type { CatalogItem, CatalogSlug } from '../types'

const TABS: { slug: CatalogSlug; label: string; singular: string }[] = [
  { slug: 'expense-types', label: 'Tipos de egreso', singular: 'tipo de egreso' },
  { slug: 'income-types', label: 'Tipos de ingreso', singular: 'tipo de ingreso' },
  { slug: 'categories', label: 'Renglones', singular: 'renglón' },
  { slug: 'payment-types', label: 'Tipos de pago', singular: 'tipo de pago' },
]

export default function Catalogs() {
  const [tab, setTab] = useState(TABS[0])
  const [owner, setOwner] = useState('') // '' = my account (only an admin can change it)
  const [items, setItems] = useState<CatalogItem[]>([])
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState('')

  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<CatalogItem | null>(null)
  const [description, setDescription] = useState('')
  const [requiresCard, setRequiresCard] = useState(false)
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const isPayment = tab.slug === 'payment-types'

  const load = useCallback(async (slug: CatalogSlug) => {
    setLoading(true)
    try {
      setItems(await getCatalog(slug, owner || undefined))
      setPageError('')
    } catch (err) {
      setPageError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [owner])

  useEffect(() => { load(tab.slug) }, [tab, load])

  const openForm = (item: CatalogItem | null) => {
    setEditing(item)
    setDescription(item?.description ?? '')
    setRequiresCard(item?.requiresCard ?? false)
    setFormError('')
    setOpen(true)
  }

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setFormError('')
    setSaving(true)
    try {
      const data = { description, requiresCard }
      if (editing) await updateCatalogItem(tab.slug, editing.id, data, owner || undefined)
      else await createCatalogItem(tab.slug, data, owner || undefined)
      setOpen(false)
      await load(tab.slug)
    } catch (err) {
      setFormError(getErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (item: CatalogItem) => {
    try {
      await setCatalogStatus(tab.slug, item.id, !item.isActive, owner || undefined)
      await load(tab.slug)
    } catch (err) {
      setPageError(getErrorMessage(err))
    }
  }

  return (
    <div>
      <PageHeader title="Catálogos" subtitle="Tus listas personales: las usas al crear egresos, ingresos y al registrar pagos."
        actionLabel="Nuevo" onAction={() => openForm(null)} />

      <OwnerSelect value={owner} onChange={setOwner} />

      <div className="mb-4 flex gap-1 overflow-x-auto border-b border-line" role="tablist">
        {TABS.map((t) => (
          <button key={t.slug} role="tab" aria-selected={t.slug === tab.slug} onClick={() => setTab(t)}
            className={`-mb-px whitespace-nowrap border-b-2 px-4 py-2 text-sm font-semibold ${
              t.slug === tab.slug ? 'border-brand text-brand-text' : 'border-transparent text-muted hover:text-ink'}`}>
            {t.label}
          </button>
        ))}
      </div>

      <ErrorBox message={pageError} />

      <div className="mt-4 overflow-x-auto rounded-xl border border-line bg-surface">
        <table className="w-full min-w-[480px] text-left text-sm">
          <thead className="border-b border-line text-muted">
            <tr>
              <th className="px-4 py-3 font-semibold">Descripción</th>
              {isPayment && <th className="px-4 py-3 font-semibold">Requiere tarjeta</th>}
              <th className="px-4 py-3 font-semibold">Estado</th>
              <th className="px-4 py-3" />
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={4} className="px-4 py-10 text-center text-muted"><Loader2 className="mx-auto animate-spin" /></td></tr>
            ) : items.length === 0 ? (
              <tr><td colSpan={4} className="px-4 py-10 text-center text-muted">
                No hay elementos. Crea el primero con “Nuevo”.
              </td></tr>
            ) : items.map((i) => (
              <tr key={i.id} className="border-b border-line last:border-0">
                <td className="px-4 py-3 font-semibold">{i.description}</td>
                {isPayment && <td className="px-4 py-3">{i.requiresCard ? 'Sí' : 'No'}</td>}
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
        <Modal title={`${editing ? 'Editar' : 'Nuevo'} ${tab.singular}`} onClose={() => setOpen(false)}>
          <form onSubmit={onSubmit} className="space-y-4">
            <Field label="Descripción">
              <input className={inputClass} value={description} onChange={(e) => setDescription(e.target.value)}
                required minLength={2} maxLength={100} autoFocus />
            </Field>
            {isPayment && (
              <label className="flex items-center gap-2 text-sm font-medium">
                <input type="checkbox" checked={requiresCard} onChange={(e) => setRequiresCard(e.target.checked)}
                  className="size-4 accent-brand" />
                Pide el número de tarjeta al registrar la transacción
              </label>
            )}
            <ErrorBox message={formError} />
            <FormActions saving={saving} label={editing ? 'Guardar cambios' : 'Crear'} onCancel={() => setOpen(false)} />
          </form>
        </Modal>
      )}
    </div>
  )
}
