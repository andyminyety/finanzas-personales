import { Moon, Sun } from 'lucide-react'
import { useTheme } from '../context/ThemeContext'

interface Props {
  /** Show the text next to the icon (sidebar). Icon only otherwise. */
  showLabel?: boolean
  className?: string
}

export default function ThemeToggle({ showLabel = false, className = '' }: Props) {
  const { theme, toggle } = useTheme()
  const isDark = theme === 'dark'
  const label = isDark ? 'Tema claro' : 'Tema oscuro'

  return (
    <button onClick={toggle} aria-label={`Cambiar a ${label.toLowerCase()}`} title={label}
      className={`flex items-center gap-3 rounded-lg text-sm font-semibold text-muted transition-colors hover:bg-paper hover:text-ink ${
        showLabel ? 'w-full px-3 py-2' : 'p-2'} ${className}`}>
      {isDark ? <Sun size={showLabel ? 18 : 20} /> : <Moon size={showLabel ? 18 : 20} />}
      {showLabel && label}
    </button>
  )
}
