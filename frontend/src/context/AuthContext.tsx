import { createContext, useContext, useState, type ReactNode } from 'react'
import { login as loginRequest } from '../api/auth'
import { TOKEN_KEY, USER_KEY } from '../api/axios'
import type { User } from '../types'

interface AuthContextValue {
  user: User | null
  isAdmin: boolean
  login: (identifier: string, password: string) => Promise<void>
  logout: () => void
  setUser: (u: User) => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function readStoredUser(): User | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw && localStorage.getItem(TOKEN_KEY) ? (JSON.parse(raw) as User) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUserState] = useState<User | null>(readStoredUser)

  const setUser = (u: User) => {
    localStorage.setItem(USER_KEY, JSON.stringify(u))
    setUserState(u)
  }

  const login = async (identifier: string, password: string) => {
    const res = await loginRequest(identifier, password)
    localStorage.setItem(TOKEN_KEY, res.token)
    setUser(res.user)
  }

  const logout = () => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    setUserState(null)
  }

  return (
    <AuthContext.Provider value={{ user, isAdmin: user?.role === 'Admin', login, logout, setUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth debe usarse dentro de AuthProvider')
  return ctx
}
