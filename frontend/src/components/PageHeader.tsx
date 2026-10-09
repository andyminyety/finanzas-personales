import { Plus } from 'lucide-react'

interface Props {
  title: string
  subtitle: string
  actionLabel?: string
  onAction?: () => void
}

export default function PageHeader({ title, subtitle, actionLabel, onAction }: Props) {
  return (
    <div className="mb-6 flex items-center justify-between gap-4">
      <div>
        <h1 className="text-2xl font-extrabold tracking-tight">{title}</h1>
        <p className="mt-1 text-sm text-muted">{subtitle}</p>
      </div>
      {actionLabel && (
        <button onClick={onAction}
          className="flex shrink-0 items-center gap-2 rounded-lg bg-brand px-4 py-2 text-sm font-bold text-white hover:bg-brand-dark">
          <Plus size={16} /> {actionLabel}
        </button>
      )}
    </div>
  )
}
