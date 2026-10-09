import { Pencil, Power } from 'lucide-react'

interface Props {
  name: string
  isActive: boolean
  onEdit: () => void
  onToggle: () => void
}

export default function RowActions({ name, isActive, onEdit, onToggle }: Props) {
  return (
    <div className="flex justify-end gap-1">
      <button onClick={onEdit} aria-label={`Editar ${name}`} title="Editar"
        className="rounded-md p-2 text-muted hover:bg-paper hover:text-ink">
        <Pencil size={16} />
      </button>
      <button onClick={onToggle} aria-label={`${isActive ? 'Inhabilitar' : 'Habilitar'} ${name}`}
        title={isActive ? 'Inhabilitar' : 'Habilitar'}
        className={`rounded-md p-2 text-muted ${
          isActive ? 'hover:bg-danger-soft hover:text-danger' : 'hover:bg-brand-soft hover:text-brand'}`}>
        <Power size={16} />
      </button>
    </div>
  )
}
