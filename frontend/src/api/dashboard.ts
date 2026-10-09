import api from './axios'
import type { Dashboard } from '../types'

/** userId: undefined = me · "all" = every user (admin) · id = that user (admin) */
export const getDashboard = (userId?: string) =>
  api.get<Dashboard>('/dashboard', { params: userId ? { userId } : {} }).then((r) => r.data)
