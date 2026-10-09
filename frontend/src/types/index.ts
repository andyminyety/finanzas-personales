export type Role = 'Admin' | 'User'
export type PersonType = 'Fisica' | 'Juridica'

export interface User {
  id: string
  name: string
  cedulaRnc: string
  email: string
  role: Role
  personType: PersonType
  expenseLimit: number
  cutDay: number
  isActive: boolean
}

export interface AuthResponse {
  token: string
  expiresAt: string
  user: User
}

export interface UserForm {
  name: string
  cedulaRnc: string
  email: string
  password: string
  expenseLimit: number
  personType: PersonType
  cutDay: number
  role: Role
}

export type CatalogSlug = 'expense-types' | 'income-types' | 'categories' | 'payment-types'

export interface CatalogItem {
  id: string
  description: string
  requiresCard: boolean
  isActive: boolean
}

export interface ExpenseItem {
  id: string
  expenseTypeId: string
  expenseTypeName: string
  categoryId: string
  categoryName: string
  defaultPaymentTypeId: string
  defaultPaymentTypeName: string
  description: string
  isActive: boolean
}

export interface IncomeItem {
  id: string
  incomeTypeId: string
  incomeTypeName: string
  description: string
  source: string
  isActive: boolean
}

export type TransactionType = 'Income' | 'Expense'

export interface Transaction {
  id: string
  number: number
  type: TransactionType
  userId: string
  userName: string
  itemId: string
  itemDescription: string
  expenseTypeId?: string | null
  expenseTypeName?: string | null
  categoryId?: string | null
  categoryName?: string | null
  incomeTypeName?: string | null
  source?: string | null
  paymentTypeId: string
  paymentTypeName: string
  transactionDate: string
  registeredAt: string
  amount: number
  cardLast4?: string | null
  comment?: string | null
  isActive: boolean
}

export interface LimitWarning {
  message: string
  limit: number
  spent: number
  percent: number
  periodStart: string
  periodEnd: string
}

export interface TransactionResult {
  transaction: Transaction
  warning?: LimitWarning | null
}

export interface TransactionPage {
  items: Transaction[]
  total: number
  page: number
  pageSize: number
  totalIncome: number
  totalExpense: number
}

export interface TransactionFilters {
  userId?: string
  from?: string
  to?: string
  type?: TransactionType | ''
  expenseTypeId?: string
  categoryId?: string
  paymentTypeId?: string
  isActive?: boolean
  page?: number
  pageSize?: number
}

export interface MonthlyCut {
  id: string
  userId: string
  userName: string
  year: number
  month: number
  cutDate: string
  periodStart: string
  periodEnd: string
  initialBalance: number
  totalIncome: number
  totalExpense: number
  finalBalance: number
  source: 'Auto' | 'Manual'
}

export interface RunCutsResult {
  created: MonthlyCut[]
  usersProcessed: number
  message: string
}

export interface NameAmount {
  name: string
  amount: number
  percent: number
}

export interface EvolutionPoint {
  year: number
  month: number
  income: number
  expense: number
}

export interface Dashboard {
  scope: string
  isAllUsers: boolean
  periodStart: string
  periodEnd: string
  currentBalance: number
  periodIncome: number
  periodExpense: number
  hasLimit: boolean
  expenseLimit: number
  limitUsedPercent: number
  expensesByCategory: NameAmount[]
  incomesByType: NameAmount[]
  evolution: EvolutionPoint[]
  lastTransactions: Transaction[]
}

export type ReportKind = 'range' | 'by-user' | 'by-type' | 'by-category' | 'monthly'

export interface ReportRow {
  label: string
  income: number
  expense: number
  balance: number
  count: number
  percent: number
}

export interface Report {
  kind: ReportKind
  title: string
  scope: string
  from?: string | null
  to?: string | null
  year?: number | null
  totals: { income: number; expense: number; balance: number; count: number }
  rows: ReportRow[]
  transactions: Transaction[]
}

export interface ReportFilters {
  userId?: string
  from?: string
  to?: string
  year?: number
}
