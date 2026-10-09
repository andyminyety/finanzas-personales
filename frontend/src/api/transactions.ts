import api from './axios'
import type { Transaction, TransactionFilters, TransactionPage, TransactionResult, TransactionType } from '../types'

export interface TransactionForm {
  type: TransactionType
  userId?: string
  itemId: string
  paymentTypeId: string
  transactionDate: string
  amount: number
  cardNumber?: string
  comment?: string
}

export const getTransactions = (filters: TransactionFilters) => {
  // Empty filters are omitted so we do not send ?type=&categoryId=...
  const params = Object.fromEntries(
    Object.entries(filters).filter(([, v]) => v !== undefined && v !== ''),
  )
  return api.get<TransactionPage>('/transactions', { params }).then((r) => r.data)
}

export const createTransaction = (data: TransactionForm) =>
  api.post<TransactionResult>('/transactions', data).then((r) => r.data)

export const updateTransaction = (id: string, data: Omit<TransactionForm, 'type' | 'userId'>) =>
  api.put<TransactionResult>(`/transactions/${id}`, data).then((r) => r.data)

export const setTransactionStatus = (id: string, isActive: boolean) =>
  api.patch<Transaction>(`/transactions/${id}/status`, { isActive }).then((r) => r.data)
