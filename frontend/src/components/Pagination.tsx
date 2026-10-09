import { ChevronLeft, ChevronRight } from 'lucide-react'

interface Props {
  page: number
  pageSize: number
  total: number
  onChange: (page: number) => void
}

export default function Pagination({ page, pageSize, total, onChange }: Props) {
  const pages = Math.max(1, Math.ceil(total / pageSize))
  if (total === 0) return null
  const btn = 'rounded-md p-2 text-muted hover:bg-paper hover:text-ink disabled:opacity-30 disabled:hover:bg-transparent'
  return (
    <div className="flex items-center justify-between px-1 pt-3 text-sm text-muted">
      <span>{total} {total === 1 ? 'resultado' : 'resultados'}</span>
      <div className="flex items-center gap-1">
        <button className={btn} disabled={page <= 1} onClick={() => onChange(page - 1)} aria-label="Página anterior">
          <ChevronLeft size={18} />
        </button>
        <span>Página {page} de {pages}</span>
        <button className={btn} disabled={page >= pages} onClick={() => onChange(page + 1)} aria-label="Página siguiente">
          <ChevronRight size={18} />
        </button>
      </div>
    </div>
  )
}
