import { Loader2, Pencil, Plus, Power } from 'lucide-react'
import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { createUser, getUsers, setUserStatus, updateUser } from '../api/users'
import { getErrorMessage } from '../api/axios'
import Badge from '../components/Badge'
import { ErrorBox, Field, inputClass } from '../components/Field'
import Modal from '../components/Modal'
import { useAuth } from '../context/AuthContext'
import type { User, UserForm } from '../types'
import { formatCedulaRnc, money } from '../utils/format'

const emptyForm: UserForm = {
  name: '', cedulaRnc: '', email: '', password: '',
  expenseLimit: 0, personType: 'Fisica', cutDay: 1, role: 'User',
}

export default function Users() {
  const { user: me } = useAuth()
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState('')

  const [editing, setEditing] = useState<User | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<UserForm>(emptyForm)
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    try {
      setUsers(await getUsers())
      setPageError('')
    } catch (err) {
      setPageError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const openCreate = () => {
    setForm(emptyForm); setFormError(''); setEditing(null); setCreating(true)
  }
  const openEdit = (u: User) => {
    setForm({ ...emptyForm, ...u, password: '' }); setFormError(''); setCreating(false); setEditing(u)
  }
  const close = () => { setCreating(false); setEditing(null) }

  const set = <K extends keyof UserForm>(key: K, value: UserForm[K]) =>
    setForm((f) => ({ ...f, [key]: value }))

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setFormError('')
    setSaving(true)
    try {
      if (editing) {
        const { password: _unused, ...rest } = form
        void _unused
        await updateUser(editing.id, rest)
      } else {
        await createUser(form)
      }
      close()
      await load()
    } catch (err) {
      setFormError(getErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (u: User) => {
    try {
      await setUserStatus(u.id, !u.isActive)
      await load()
    } catch (err) {
      setPageError(getErrorMessage(err))
    }
  }

  const isJuridica = form.personType === 'Juridica'

  return (
    <div>
      <div className="mb-6 flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-extrabold tracking-tight">Usuarios</h1>
          <p className="mt-1 text-sm text-muted">Crea, edita y habilita o inhabilita usuarios.</p>
        </div>
        <button onClick={openCreate}
          className="flex items-center gap-2 rounded-lg bg-brand px-4 py-2 text-sm font-bold text-white hover:bg-brand-dark">
          <Plus size={16} /> Nuevo usuario
        </button>
      </div>

      <ErrorBox message={pageError} />

      <div className="mt-4 overflow-x-auto rounded-xl border border-line bg-white">
        <table className="w-full min-w-[760px] text-left text-sm">
          <thead className="border-b border-line text-muted">
            <tr>
              {['Nombre', 'Cédula / RNC', 'Tipo', 'Límite', 'Corte', 'Rol', 'Estado', ''].map((h) => (
                <th key={h} className="px-4 py-3 font-semibold">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={8} className="px-4 py-10 text-center text-muted">
                <Loader2 className="mx-auto animate-spin" />
              </td></tr>
            ) : users.length === 0 ? (
              <tr><td colSpan={8} className="px-4 py-10 text-center text-muted">
                Aún no hay usuarios. Crea el primero con “Nuevo usuario”.
              </td></tr>
            ) : users.map((u) => (
              <tr key={u.id} className="border-b border-line last:border-0">
                <td className="px-4 py-3">
                  <p className="font-semibold">{u.name}</p>
                  <p className="text-xs text-muted">{u.email}</p>
                </td>
                <td className="px-4 py-3">{formatCedulaRnc(u.cedulaRnc)}</td>
                <td className="px-4 py-3">{u.personType === 'Fisica' ? 'Física' : 'Jurídica'}</td>
                <td className="px-4 py-3">{money(u.expenseLimit)}</td>
                <td className="px-4 py-3">Día {u.cutDay}</td>
                <td className="px-4 py-3">{u.role === 'Admin' ? 'Admin' : 'Usuario'}</td>
                <td className="px-4 py-3"><Badge active={u.isActive} /></td>
                <td className="px-4 py-3">
                  <div className="flex justify-end gap-1">
                    <button onClick={() => openEdit(u)} aria-label={`Editar ${u.name}`} title="Editar"
                      className="rounded-md p-2 text-muted hover:bg-paper hover:text-ink">
                      <Pencil size={16} />
                    </button>
                    <button onClick={() => toggle(u)} disabled={u.id === me?.id}
                      aria-label={u.isActive ? `Inhabilitar ${u.name}` : `Habilitar ${u.name}`}
                      title={u.isActive ? 'Inhabilitar' : 'Habilitar'}
                      className={`rounded-md p-2 disabled:opacity-30 ${
                        u.isActive ? 'text-muted hover:bg-danger-soft hover:text-danger'
                                   : 'text-muted hover:bg-brand-soft hover:text-brand'}`}>
                      <Power size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {(creating || editing) && (
        <Modal title={editing ? 'Editar usuario' : 'Nuevo usuario'} onClose={close}>
          <form onSubmit={onSubmit} className="space-y-4">
            <Field label="Nombre">
              <input className={inputClass} value={form.name} onChange={(e) => set('name', e.target.value)} required />
            </Field>

            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Tipo de persona">
                <select className={inputClass} value={form.personType}
                  onChange={(e) => set('personType', e.target.value as UserForm['personType'])}>
                  <option value="Fisica">Física</option>
                  <option value="Juridica">Jurídica</option>
                </select>
              </Field>
              <Field label={isJuridica ? 'RNC (9 dígitos)' : 'Cédula (11 dígitos)'}>
                <input className={inputClass} value={form.cedulaRnc}
                  onChange={(e) => set('cedulaRnc', e.target.value)} inputMode="numeric" required />
              </Field>
            </div>

            <Field label="Correo">
              <input type="email" className={inputClass} value={form.email}
                onChange={(e) => set('email', e.target.value)} required />
            </Field>

            {!editing && (
              <Field label="Contraseña inicial (mínimo 6 caracteres)">
                <input type="password" className={inputClass} value={form.password}
                  onChange={(e) => set('password', e.target.value)} minLength={6} required autoComplete="new-password" />
              </Field>
            )}

            <div className="grid gap-4 sm:grid-cols-3">
              <Field label="Límite de egresos">
                <input type="number" min={0} step="0.01" className={inputClass} value={form.expenseLimit}
                  onChange={(e) => set('expenseLimit', Number(e.target.value))} required />
              </Field>
              <Field label="Día de corte">
                <input type="number" min={1} max={28} className={inputClass} value={form.cutDay}
                  onChange={(e) => set('cutDay', Number(e.target.value))} required />
              </Field>
              <Field label="Rol">
                <select className={inputClass} value={form.role}
                  onChange={(e) => set('role', e.target.value as UserForm['role'])}>
                  <option value="User">Usuario</option>
                  <option value="Admin">Administrador</option>
                </select>
              </Field>
            </div>

            <ErrorBox message={formError} />

            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={close}
                className="rounded-lg px-4 py-2 text-sm font-semibold text-muted hover:bg-paper">
                Cancelar
              </button>
              <button disabled={saving}
                className="flex items-center gap-2 rounded-lg bg-brand px-4 py-2 text-sm font-bold text-white hover:bg-brand-dark disabled:opacity-60">
                {saving && <Loader2 size={16} className="animate-spin" />}
                {editing ? 'Guardar cambios' : 'Crear usuario'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}
