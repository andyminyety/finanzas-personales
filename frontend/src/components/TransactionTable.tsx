import { ArrowDownRight, ArrowUpRight, Loader2 } from 'lucide-react'
import type { ReactNode } from 'react'
import type { Transaction } from '../types'
import { formatDate, money, txNumber } from '../utils/format'
import Badge from './Badge'

interface Props {
  rows: Transaction[]
  loading: boolean
  showUser: boolean
  emptyText: string
  actions?: (t: Transaction) => ReactNode
}

export default function TransactionTable({ rows, loading, showUser, emptyText, actions }: Props) {
  const headers = ['N.º', 'Fecha', 'Concepto', ...(showUser ? ['Usuario'] : []), 'Pago', 'Monto', 'Estado', ...(actions ? [''] : [])]
  return (
    <div className="overflow-x-auto rounded-xl border border-line bg-white">
      <table className="w-full min-w-[820px] text-left text-sm">
        <thead className="border-b border-line text-muted">
          <tr>{headers.map((h, i) => <th key={i} className="px-4 py-3 font-semibold">{h}</th>)}</tr>
        </thead>
        <tbody>
          {loading ? (
            <tr><td colSpan={headers.length} className="px-4 py-10 text-center text-muted">
              <Loader2 className="mx-auto animate-spin" />
            </td></tr>
          ) : rows.length === 0 ? (
            <tr><td colSpan={headers.length} className="px-4 py-10 text-center text-muted">{emptyText}</td></tr>
          ) : rows.map((t) => {
            const income = t.type === 'Income'
            return (
              <tr key={t.id} className={`border-b border-line last:border-0 ${t.isActive ? '' : 'opacity-60'}`}>
                <td className="whitespace-nowrap px-4 py-3 text-muted">{txNumber(t.number)}</td>
                <td className="whitespace-nowrap px-4 py-3">{formatDate(t.transactionDate)}</td>
                <td className="px-4 py-3">
                  <p className="flex items-center gap-1.5 font-semibold">
                    {income
                      ? <ArrowUpRight size={15} className="text-brand" aria-label="Ingreso" />
                      : <ArrowDownRight size={15} className="text-danger" aria-label="Egreso" />}
                    {t.itemDescription}
                  </p>
                  <p className="text-xs text-muted">
                    {income ? `${t.incomeTypeName} · ${t.source}` : `${t.categoryName} · ${t.expenseTypeName}`}
                  </p>
                  {t.comment && <p className="mt-0.5 max-w-xs truncate text-xs text-muted" title={t.comment}>“{t.comment}”</p>}
                </td>
                {showUser && <td className="px-4 py-3">{t.userName}</td>}
                <td className="px-4 py-3">
                  {t.paymentTypeName}
                  {t.cardLast4 && <span className="text-xs text-muted"> ····{t.cardLast4}</span>}
                </td>
                <td className={`whitespace-nowrap px-4 py-3 font-bold ${income ? 'text-brand-dark' : 'text-danger'}`}>
                  {income ? '+' : '−'}{money(t.amount)}
                </td>
                <td className="px-4 py-3"><Badge active={t.isActive} /></td>
                {actions && <td className="px-4 py-3">{actions(t)}</td>}
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
