import { Check, KeyRound, Loader2, Lock, Save } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { changePassword, updateProfile } from '../api/auth'
import { getErrorMessage } from '../api/axios'
import { ErrorBox, Field, inputClass } from '../components/Field'
import { useAuth } from '../context/AuthContext'
import type { PersonType } from '../types'
import { formatCedulaRnc, money } from '../utils/format'

export default function Profile() {
  const { user, isAdmin, setUser } = useAuth()
  const [name, setName] = useState(user?.name ?? '')
  const [email, setEmail] = useState(user?.email ?? '')
  const [personType, setPersonType] = useState<PersonType>(user?.personType ?? 'Fisica')
  const [cedulaRnc, setCedulaRnc] = useState(user?.cedulaRnc ?? '')
  const [limit, setLimit] = useState(user?.expenseLimit ?? 0)
  const [cutDay, setCutDay] = useState(user?.cutDay ?? 1)
  const [profileMsg, setProfileMsg] = useState({ ok: '', err: '' })
  const [savingProfile, setSavingProfile] = useState(false)

  const [current, setCurrent] = useState('')
  const [next, setNext] = useState('')
  const [pwdMsg, setPwdMsg] = useState({ ok: '', err: '' })
  const [savingPwd, setSavingPwd] = useState(false)

  if (!user) return null

  const saveProfile = async (e: FormEvent) => {
    e.preventDefault()
    setProfileMsg({ ok: '', err: '' })
    setSavingProfile(true)
    try {
      const updated = await updateProfile({
        name, email,
        // Any user can adjust their limit and cut day; identity fields only an administrator.
        expenseLimit: limit,
        cutDay,
        ...(isAdmin ? { personType, cedulaRnc } : {}),
      })
      setUser(updated)
      setCedulaRnc(updated.cedulaRnc)
      setProfileMsg({ ok: 'Cambios guardados.', err: '' })
    } catch (err) {
      setProfileMsg({ ok: '', err: getErrorMessage(err) })
    } finally {
      setSavingProfile(false)
    }
  }

  const savePassword = async (e: FormEvent) => {
    e.preventDefault()
    setPwdMsg({ ok: '', err: '' })
    setSavingPwd(true)
    try {
      await changePassword({ currentPassword: current, newPassword: next })
      setCurrent('')
      setNext('')
      setPwdMsg({ ok: 'Contraseña actualizada.', err: '' })
    } catch (err) {
      setPwdMsg({ ok: '', err: getErrorMessage(err) })
    } finally {
      setSavingPwd(false)
    }
  }

  const Ok = ({ text }: { text: string }) =>
    text ? (
      <p className="flex items-center gap-2 rounded-lg bg-brand-soft px-3 py-2 text-sm font-medium text-brand-text">
        <Check size={16} /> {text}
      </p>
    ) : null

  const button = 'flex items-center gap-2 rounded-lg bg-brand px-4 py-2 text-sm font-bold text-white hover:bg-brand-dark disabled:opacity-60'

  return (
    <div className="max-w-2xl space-y-6">
      <h1 className="text-2xl font-extrabold tracking-tight">Mi perfil</h1>

      <section className="rounded-xl border border-line bg-surface p-6">
        <form onSubmit={saveProfile} className="space-y-4">
          <h2 className="font-bold">Datos personales</h2>
          <Field label="Nombre">
            <input className={inputClass} value={name} onChange={(e) => setName(e.target.value)} required />
          </Field>
          <Field label="Correo">
            <input type="email" className={inputClass} value={email} onChange={(e) => setEmail(e.target.value)} required />
          </Field>

          {isAdmin ? (
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Tipo de persona">
                <select className={inputClass} value={personType} onChange={(e) => setPersonType(e.target.value as PersonType)}>
                  <option value="Fisica">Física</option>
                  <option value="Juridica">Jurídica</option>
                </select>
              </Field>
              <Field label={personType === 'Juridica' ? 'RNC (9 dígitos)' : 'Cédula (11 dígitos)'}>
                <input className={inputClass} inputMode="numeric" value={cedulaRnc}
                  onChange={(e) => setCedulaRnc(e.target.value)} required />
              </Field>
            </div>
          ) : (
            <>
              <Field label="Cédula / RNC">
                <input className={inputClass} value={formatCedulaRnc(user.cedulaRnc)} disabled />
              </Field>
              <p className="-mt-2 flex items-center gap-2 text-xs text-muted">
                <Lock size={13} /> La cédula/RNC solo la puede cambiar un administrador.
              </p>
            </>
          )}

          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Límite mensual de egresos (0 = sin límite)">
              <input type="number" min={0} step="0.01" className={inputClass} value={limit}
                onChange={(e) => setLimit(Number(e.target.value))} required />
            </Field>
            <Field label="Día de corte (1-28)">
              <input type="number" min={1} max={28} className={inputClass} value={cutDay}
                onChange={(e) => setCutDay(Number(e.target.value))} required />
            </Field>
          </div>
          <p className="-mt-2 text-xs text-muted">
            Ajústalos a tu vida: el límite ({money(limit)}) activa el aviso al gastar de más, y el día de corte
            cierra cada período. Si cambias el día de corte, aplica a los próximos cortes; los ya generados no cambian.
          </p>

          <ErrorBox message={profileMsg.err} />
          <Ok text={profileMsg.ok} />
          <button disabled={savingProfile} className={button}>
            {savingProfile ? <Loader2 size={16} className="animate-spin" /> : <Save size={16} />}
            Guardar cambios
          </button>
        </form>
      </section>

      <section className="rounded-xl border border-line bg-surface p-6">
        <form onSubmit={savePassword} className="space-y-4">
          <h2 className="font-bold">Cambiar contraseña</h2>
          <Field label="Contraseña actual">
            <input type="password" className={inputClass} value={current} onChange={(e) => setCurrent(e.target.value)}
              autoComplete="current-password" required />
          </Field>
          <Field label="Nueva contraseña (mínimo 6 caracteres)">
            <input type="password" className={inputClass} value={next} onChange={(e) => setNext(e.target.value)}
              autoComplete="new-password" minLength={6} required />
          </Field>
          <ErrorBox message={pwdMsg.err} />
          <Ok text={pwdMsg.ok} />
          <button disabled={savingPwd} className={button}>
            {savingPwd ? <Loader2 size={16} className="animate-spin" /> : <KeyRound size={16} />}
            Actualizar contraseña
          </button>
        </form>
      </section>
    </div>
  )
}
