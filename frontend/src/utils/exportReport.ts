import type { Report } from '../types'
import { buildReportTable, periodText } from './reportTable'

/** Monto en texto plano para PDF (la fuente base de PDF no soporta todos los símbolos). */
const pdfMoney = (n: number) =>
  `RD$ ${n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

const fileName = (r: Report, ext: string) => `reporte-${r.kind}-${new Date().toLocaleDateString('en-CA')}.${ext}`

export function exportReportCsv(report: Report, showUser: boolean) {
  const t = buildReportTable(report, (n) => n.toFixed(2), showUser)
  const esc = (v: string) => `"${v.replace(/"/g, '""')}"`
  const lines = [
    [report.title], [`Usuario: ${report.scope}`], [periodText(report)], [],
    t.columns.map((c) => c.label), ...t.rows, ...(t.footer ? [t.footer] : []),
  ]
  const csv = lines.map((l) => l.map(esc).join(',')).join('\r\n')
  // El BOM hace que Excel abra bien los acentos.
  const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName(report, 'csv')
  a.click()
  URL.revokeObjectURL(url)
}

export async function exportReportPdf(report: Report, showUser: boolean) {
  // Se carga solo al exportar para no engordar la carga inicial de la app.
  const { buildReportPdf } = await import('./pdf')
  const table = buildReportTable(report, pdfMoney, showUser)
  const doc = buildReportPdf(report, table, pdfMoney, periodText(report))
  doc.save(fileName(report, 'pdf'))
}
