import type { Report } from '../types'
import { formatDate, txNumber } from './format'

export interface ReportColumn {
  label: string
  align: 'left' | 'right'
}

export interface ReportTable {
  columns: ReportColumn[]
  rows: string[][]
  footer?: string[]
}

const L = (label: string): ReportColumn => ({ label, align: 'left' })
const R = (label: string): ReportColumn => ({ label, align: 'right' })

/**
 * Convierte un reporte en una tabla de textos. La misma tabla se usa en pantalla, en el PDF y en
 * el CSV, así los tres siempre muestran exactamente lo mismo. `fmt` decide cómo se escriben los montos.
 */
export function buildReportTable(r: Report, fmt: (n: number) => string, showUser: boolean): ReportTable {
  switch (r.kind) {
    case 'range':
      return {
        columns: [L('N.º'), L('Fecha'), L('Concepto'), ...(showUser ? [L('Usuario')] : []), L('Tipo'), L('Pago'), R('Monto')],
        rows: r.transactions.map((t) => [
          txNumber(t.number),
          formatDate(t.transactionDate),
          t.itemDescription,
          ...(showUser ? [t.userName] : []),
          t.type === 'Income' ? 'Ingreso' : 'Egreso',
          t.paymentTypeName,
          (t.type === 'Income' ? '' : '-') + fmt(t.amount),
        ]),
      }

    case 'by-user':
      return {
        columns: [L('Usuario'), R('Ingresos'), R('Egresos'), R('Balance'), R('Transacciones')],
        rows: r.rows.map((x) => [x.label, fmt(x.income), fmt(x.expense), fmt(x.balance), String(x.count)]),
        footer: ['Total', fmt(r.totals.income), fmt(r.totals.expense), fmt(r.totals.balance), String(r.totals.count)],
      }

    case 'by-type':
      return {
        columns: [L('Tipo de transacción'), R('Monto'), R('Transacciones')],
        rows: r.rows.map((x) => [x.label, fmt(x.label === 'Ingresos' ? x.income : x.expense), String(x.count)]),
      }

    case 'by-category':
      return {
        columns: [L('Categoría'), R('Monto'), R('% de los egresos'), R('Transacciones')],
        rows: r.rows.map((x) => [x.label, fmt(x.expense), `${x.percent}%`, String(x.count)]),
        footer: ['Total', fmt(r.totals.expense), r.rows.length ? '100%' : '0%', String(r.rows.reduce((a, x) => a + x.count, 0))],
      }

    case 'monthly':
      return {
        columns: [L('Mes'), R('Ingresos'), R('Egresos'), R('Balance'), R('Transacciones')],
        rows: r.rows.map((x) => [x.label, fmt(x.income), fmt(x.expense), fmt(x.balance), String(x.count)]),
        footer: ['Total', fmt(r.totals.income), fmt(r.totals.expense), fmt(r.totals.balance), String(r.totals.count)],
      }
  }
}

/** Texto del período del reporte, para encabezados. */
export function periodText(r: Report): string {
  if (r.kind === 'monthly') return `Año ${r.year}`
  if (r.from && r.to) return `Del ${formatDate(r.from)} al ${formatDate(r.to)}`
  if (r.from) return `Desde el ${formatDate(r.from)}`
  if (r.to) return `Hasta el ${formatDate(r.to)}`
  return 'Todas las fechas'
}
