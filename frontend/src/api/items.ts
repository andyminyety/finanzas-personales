import api from './axios'
import type { ExpenseItem, IncomeItem } from '../types'

export interface ExpenseItemForm {
  expenseTypeId: string
  categoryId: string
  defaultPaymentTypeId: string
  description: string
}
export interface IncomeItemForm {
  incomeTypeId: string
  description: string
  source: string
}

// userId solo lo respeta el servidor si quien llama es administrador.
const q = (userId?: string) => (userId ? { params: { userId } } : undefined)

export const getExpenseItems = (userId?: string) => api.get<ExpenseItem[]>('/expense-items', q(userId)).then((r) => r.data)
export const createExpenseItem = (d: ExpenseItemForm, userId?: string) =>
  api.post<ExpenseItem>('/expense-items', d, q(userId)).then((r) => r.data)
export const updateExpenseItem = (id: string, d: ExpenseItemForm, userId?: string) =>
  api.put<ExpenseItem>(`/expense-items/${id}`, d, q(userId)).then((r) => r.data)
export const setExpenseItemStatus = (id: string, isActive: boolean, userId?: string) =>
  api.patch<ExpenseItem>(`/expense-items/${id}/status`, { isActive }, q(userId)).then((r) => r.data)

export const getIncomeItems = (userId?: string) => api.get<IncomeItem[]>('/income-items', q(userId)).then((r) => r.data)
export const createIncomeItem = (d: IncomeItemForm, userId?: string) =>
  api.post<IncomeItem>('/income-items', d, q(userId)).then((r) => r.data)
export const updateIncomeItem = (id: string, d: IncomeItemForm, userId?: string) =>
  api.put<IncomeItem>(`/income-items/${id}`, d, q(userId)).then((r) => r.data)
export const setIncomeItemStatus = (id: string, isActive: boolean, userId?: string) =>
  api.patch<IncomeItem>(`/income-items/${id}/status`, { isActive }, q(userId)).then((r) => r.data)
