import { Loader2, LogIn } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { getErrorMessage } from '../api/axios'
import { ErrorBox, Field, inputClass } from '../components/Field'
import Logo from '../components/Logo'
import ThemeToggle from '../components/ThemeToggle'
import { useAuth } from '../context/AuthContext'

export default function Login() {
  const { user, login } = useAuth()
  const navigate = useNavigate()
  const [identifier, setIdentifier] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  if (user) return <Navigate to="/" replace />

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await login(identifier, password)
      navigate('/')
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      <div className="hidden flex-col justify-between bg-brand-dark p-12 text-white lg:flex">
        <span className="flex items-center gap-2.5 font-extrabold">
          <Logo className="size-9 ring-1 ring-white/30" />
          Finanzas
        </span>
        <div>
          <h1 className="max-w-md text-4xl font-extrabold leading-tight tracking-tight">
            Cada peso con su lugar.
          </h1>
          <p className="mt-4 max-w-sm text-white/70">
            Registra ingresos y egresos, controla tu límite mensual y revisa tu corte al día.
          </p>
        </div>
        <p className="text-sm text-white/50">Sistema de Gestión de Finanzas Personales</p>
      </div>

      <div className="relative flex items-center justify-center p-6">
        <ThemeToggle className="absolute right-4 top-4" />
        <form onSubmit={onSubmit} className="w-full max-w-sm space-y-4">
          <div>
            <h2 className="text-2xl font-extrabold tracking-tight">Iniciar sesión</h2>
            <p className="mt-1 text-sm text-muted">Usa tu correo o tu cédula/RNC.</p>
          </div>

          <Field label="Correo o cédula/RNC">
            <input className={inputClass} value={identifier} onChange={(e) => setIdentifier(e.target.value)}
              autoComplete="username" required autoFocus />
          </Field>
          <Field label="Contraseña">
            <input type="password" className={inputClass} value={password}
              onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" required />
          </Field>

          <ErrorBox message={error} />

          <button disabled={loading}
            className="flex w-full items-center justify-center gap-2 rounded-lg bg-brand px-4 py-2.5 text-sm font-bold text-white hover:bg-brand-dark disabled:opacity-60">
            {loading ? <Loader2 size={16} className="animate-spin" /> : <LogIn size={16} />}
            Entrar
          </button>
        </form>
      </div>
    </div>
  )
}
