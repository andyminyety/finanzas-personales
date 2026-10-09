import { Loader2 } from 'lucide-react'

interface Props {
  saving: boolean
  label: string
  onCancel: () => void
}

export default function FormActions({ saving, label, onCancel }: Props) {
  return (
    <div className="flex justify-end gap-2 pt-2">
      <button type="button" onClick={onCancel}
        className="rounded-lg px-4 py-2 text-sm font-semibold text-muted hover:bg-paper">
        Cancelar
      </button>
      <button disabled={saving}
        className="flex items-center gap-2 rounded-lg bg-brand px-4 py-2 text-sm font-bold text-white hover:bg-brand-dark disabled:opacity-60">
        {saving && <Loader2 size={16} className="animate-spin" />}
        {label}
      </button>
    </div>
  )
}
