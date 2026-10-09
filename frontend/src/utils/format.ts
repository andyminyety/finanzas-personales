export const money = (n: number) =>
  new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(n)

/** Formats as 001-1234567-8 (cédula) or 1-01-12345-6 (RNC) */
export const formatCedulaRnc = (v: string) => {
  if (v.length === 11) return `${v.slice(0, 3)}-${v.slice(3, 10)}-${v.slice(10)}`
  if (v.length === 9) return `${v.slice(0, 1)}-${v.slice(1, 3)}-${v.slice(3, 8)}-${v.slice(8)}`
  return v
}

/** "2026-10-04T00:00:00Z" → "04/10/2026" (no time zone conversion) */
export const formatDate = (iso: string) => {
  const [y, m, d] = iso.slice(0, 10).split('-')
  return `${d}/${m}/${y}`
}

export const txNumber = (n: number) => `T-${String(n).padStart(6, '0')}`

/** Local date for today as yyyy-mm-dd (for <input type="date">) */
export const todayISO = () => new Date().toLocaleDateString('en-CA')

const MONTHS = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre']
export const monthLabel = (year: number, month: number) => `${MONTHS[month - 1]} ${year}`

const MONTHS_SHORT = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic']
export const monthShort = (year: number, month: number) => `${MONTHS_SHORT[month - 1]} ${String(year).slice(2)}`

/** For chart axes: 12,500 → 12.5k */
export const compactMoney = (n: number) =>
  new Intl.NumberFormat('es-DO', { notation: 'compact', maximumFractionDigits: 1 }).format(n)
