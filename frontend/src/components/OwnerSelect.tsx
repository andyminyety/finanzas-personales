import { UserCog } from 'lucide-react'
import { useEffect, useState } from 'react'
import { getUsers } from '../api/users'
import { useAuth } from '../context/AuthContext'
import type { User } from '../types'
import { inputClass } from './Field'

interface Props {
  value: string // '' = my account
  onChange: (userId: string) => void
}

/** Administrators only: lets them manage another user's catalogs, expense items and income items. */
export default function OwnerSelect({ value, onChange }: Props) {
  const { user, isAdmin } = useAuth()
  const [users, setUsers] = useState<User[]>([])

  useEffect(() => {
    if (isAdmin) getUsers().then(setUsers).catch(() => undefined)
  }, [isAdmin])

  if (!isAdmin) return null

  return (
    <div className="mb-4 flex flex-wrap items-center gap-3 rounded-xl border border-line bg-surface p-3">
      <label htmlFor="owner-select" className="flex items-center gap-2 text-sm font-semibold">
        <UserCog size={16} className="text-muted" /> Administrando los datos de
      </label>
      <select id="owner-select" className={`${inputClass} max-w-64`} value={value} onChange={(e) => onChange(e.target.value)}>
        <option value="">Mi cuenta</option>
        {users.filter((u) => u.id !== user?.id).map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
      </select>
    </div>
  )
}
