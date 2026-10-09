import api from './axios'
import type { Dashboard } from '../types'

/** userId: undefined = yo · "all" = todos (admin) · id = ese usuario (admin) */
export const getDashboard = (userId?: string) =>
  api.get<Dashboard>('/dashboard', { params: userId ? { userId } : {} }).then((r) => r.data)
