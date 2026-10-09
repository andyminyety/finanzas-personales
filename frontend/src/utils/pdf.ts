import { jsPDF } from 'jspdf'
import autoTable from 'jspdf-autotable'
import type { Report } from '../types'
import type { ReportTable } from './reportTable'

const BRAND: [number, number, number] = [14, 107, 92]
const INK: [number, number, number] = [22, 33, 31]
const MUTED: [number, number, number] = [107, 119, 115]

export function buildReportPdf(report: Report, table: ReportTable, fmt: (n: number) => string, period: string) {
  // The transaction detail has many columns, so it is printed in landscape.
  const doc = new jsPDF({ orientation: report.kind === 'range' ? 'landscape' : 'portrait', unit: 'mm', format: 'a4' })
  const pageW = doc.internal.pageSize.getWidth()
  const margin = 14

  doc.setFont('helvetica', 'bold').setFontSize(18).setTextColor(...INK)
  doc.text(report.title, margin, 20)

  doc.setFont('helvetica', 'normal').setFontSize(10).setTextColor(...MUTED)
  doc.text(`Usuario: ${report.scope}`, margin, 27)
  doc.text(`Período: ${period}`, margin, 32)
  doc.text(`Generado: ${new Date().toLocaleString('es-DO')}`, pageW - margin, 27, { align: 'right' })

  // Summary in three boxes
  const boxes: [string, number][] = [
    ['Ingresos', report.totals.income],
    ['Egresos', report.totals.expense],
    ['Balance', report.totals.balance],
  ]
  const gap = 4
  const boxW = (pageW - margin * 2 - gap * 2) / 3
  boxes.forEach(([label, value], i) => {
    const x = margin + i * (boxW + gap)
    doc.setDrawColor(227, 231, 227).setFillColor(245, 246, 243).roundedRect(x, 38, boxW, 16, 2, 2, 'FD')
    doc.setFontSize(8).setTextColor(...MUTED).text(label, x + 3, 44)
    doc.setFont('helvetica', 'bold').setFontSize(11).setTextColor(...INK).text(fmt(value), x + 3, 50)
    doc.setFont('helvetica', 'normal')
  })

  const numeric = table.columns.reduce<Record<number, { halign: 'right' }>>((acc, c, i) => {
    if (c.align === 'right') acc[i] = { halign: 'right' }
    return acc
  }, {})

  autoTable(doc, {
    startY: 60,
    head: [table.columns.map((c) => c.label)],
    body: table.rows,
    foot: table.footer ? [table.footer] : undefined,
    showFoot: 'lastPage',
    theme: 'striped',
    styles: { font: 'helvetica', fontSize: 9, cellPadding: 2.2, textColor: INK },
    headStyles: { fillColor: BRAND, textColor: 255, fontStyle: 'bold' },
    footStyles: { fillColor: [226, 240, 237], textColor: INK, fontStyle: 'bold' },
    alternateRowStyles: { fillColor: [250, 251, 249] },
    columnStyles: numeric,
    didParseCell: (d) => {
      // Also right-align headers and footers of numeric columns.
      if (d.section !== 'body' && table.columns[d.column.index]?.align === 'right') d.cell.styles.halign = 'right'
    },
    margin: { left: margin, right: margin },
  })

  if (table.rows.length === 0) {
    doc.setFontSize(10).setTextColor(...MUTED).text('No hay datos para los filtros seleccionados.', margin, 66)
  }

  const pages = doc.getNumberOfPages()
  const pageH = doc.internal.pageSize.getHeight()
  for (let i = 1; i <= pages; i++) {
    doc.setPage(i).setFontSize(8).setTextColor(...MUTED)
    doc.text('Sistema de Gestión de Finanzas Personales', margin, pageH - 8)
    doc.text(`Página ${i} de ${pages}`, pageW - margin, pageH - 8, { align: 'right' })
  }
  return doc
}
