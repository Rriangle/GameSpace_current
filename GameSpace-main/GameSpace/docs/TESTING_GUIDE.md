# GameSpace Testing Guide

## Pre-Testing Checklist

### 1. Database Setup
```bash
# Create database
sqlcmd -S (localdb)\MSSQLLocalDB -Q "CREATE DATABASE GameSpace;"

# Run schema
sqlcmd -S (localdb)\MSSQLLocalDB -d GameSpace -i schema/database.sql
```

### 2. Application Setup
```bash
# Restore packages
dotnet restore

# Build application
dotnet build

# Run application
dotnet run
```

## Testing Areas

### 1. Public Area Testing
- **URL:** `https://localhost:5001/Public`
- **Tests:**
  - Home page loads correctly
  - Navigation links work
  - About page displays
  - Contact page displays
  - Privacy policy displays

### 2. MiniGame Area Testing
- **URL:** `https://localhost:5001/MiniGame`
- **Tests:**
  - UserWallet: View wallet, check history
  - UserSignInStats: Daily sign-in functionality
  - Pet: Pet care, feeding, playing
  - MiniGame: Start games, play, end games

### 3. Admin Area Testing
- **URL:** `https://localhost:5001/Admin`
- **Tests:**
  - Dashboard loads with statistics
  - Navigation works
  - Role-based access (Admin only)

### 4. Forum Area Testing
- **URL:** `https://localhost:5001/Forum`
- **Tests:**
  - Forum home page
  - Topic listing
  - Create new topic

### 5. MemberManagement Area Testing
- **URL:** `https://localhost:5001/MemberManagement`
- **Tests:**
  - User listing
  - User details
  - User editing

### 6. OnlineStore Area Testing
- **URL:** `https://localhost:5001/OnlineStore`
- **Tests:**
  - Store home page
  - Coupon listing
  - Evoucher listing
  - Purchase functionality

## API Testing

### Health Check Endpoints
```bash
# Basic health check
curl https://localhost:5001/api/health

# Database health check
curl https://localhost:5001/api/health/db
```

### Data Seeding
```bash
# Seed data (Admin only)
curl -X POST https://localhost:5001/api/DataSeeding/seed
```

## Database Testing

### Verify Data Seeding
```sql
-- Check all tables have 200 rows
SELECT 
    t.name AS TableName,
    p.rows AS RowCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0,1)
ORDER BY t.name;
```

### Verify Relationships
```sql
-- Check foreign key relationships
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id;
```

## Performance Testing

### Load Testing
- Test with multiple concurrent users
- Monitor database performance
- Check memory usage
- Verify response times

### Stress Testing
- Test with large datasets
- Verify error handling
- Check system stability

## Security Testing

### Authentication
- Test user login/logout
- Verify session management
- Check password requirements

### Authorization
- Test role-based access
- Verify admin-only features
- Check user permissions

### Input Validation
- Test SQL injection prevention
- Verify XSS protection
- Check CSRF protection

## Error Handling Testing

### Database Errors
- Test connection failures
- Verify error messages
- Check fallback behavior

### Application Errors
- Test invalid inputs
- Verify error pages
- Check logging

## Browser Compatibility

### Desktop Browsers
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

### Mobile Browsers
- Chrome Mobile
- Safari Mobile
- Firefox Mobile

## Test Results

### Expected Results
- All areas load correctly
- Navigation works properly
- Database operations succeed
- Error handling works
- Performance is acceptable

### Success Criteria
- 0 build errors
- 0 runtime errors
- All tests pass
- Performance meets requirements
- Security requirements met

## Troubleshooting

### Common Issues
1. **Database Connection:** Check connection strings
2. **Build Errors:** Verify NuGet packages
3. **Runtime Errors:** Check logs
4. **Performance Issues:** Monitor resources

### Log Files
- Application logs: `logs/gamespace-*.txt`
- Console output: Check terminal
- Database logs: SQL Server logs

## Test Completion Checklist

- [ ] All areas tested
- [ ] All APIs tested
- [ ] Database verified
- [ ] Performance tested
- [ ] Security tested
- [ ] Error handling tested
- [ ] Browser compatibility tested
- [ ] All issues resolved
- [ ] Documentation updated
- [ ] Ready for production